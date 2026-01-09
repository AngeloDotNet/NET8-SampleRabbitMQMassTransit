using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SampleMicroservice.Shared;

namespace SampleMicroservice.Messaging;

public class RabbitMqBus : IRabbitMqBus
{
    private readonly ConnectionFactory factory;
    private IConnection? connection;
    private IModel? channel;
    ILogger<RabbitMqBus>? logger;

    private string? replyQueueName;
    private AsyncEventingBasicConsumer? replyConsumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingRequests = new();

    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
    private bool started;

    //public RabbitMqBus(string hostName, string userName, string password, string virtualHost = "/")
    public RabbitMqBus(string hostName, string userName, string password, string virtualHost = "/", ILogger<RabbitMqBus>? logger = null)
    {
        factory = new ConnectionFactory
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            VirtualHost = virtualHost,
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };
    }

    public void Start()
    {
        if (started)
        {
            return;
        }

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        // declare exclusive auto-delete reply queue for RPC clients
        var replyQueue = channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true);

        replyQueueName = replyQueue.QueueName;
        replyConsumer = new AsyncEventingBasicConsumer(channel);
        replyConsumer.Received += async (sender, ea) =>
        {
            //if (ea.BasicProperties?.CorrelationId is string corr && pendingRequests.TryRemove(corr, out var tcs))
            //{
            //    tcs.TrySetResult(ea.Body.ToArray());
            //}
            if (ea.BasicProperties?.CorrelationId is string corr && pendingRequests.TryRemove(corr, out var tcs))
            {
                logger.LogInformation("Received RPC response with CorrelationId={Corr}", corr);
                tcs.TrySetResult(ea.Body.ToArray());
            }

            await Task.CompletedTask; // required because handler is async
        };

        channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: replyConsumer);
        started = true;
    }

    public void Stop()
    {
        if (!started)
        {
            return;
        }

        try
        {
            channel?.Close();
            connection?.Close();
        }
        catch
        {
            /* swallow */
        }
        finally
        {
            channel?.Dispose();
            connection?.Dispose();
            channel = null;
            connection = null;
            replyQueueName = null;
            replyConsumer = null;
            pendingRequests.Clear();
            started = false;
        }
    }

    public Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default)
    {
        EnsureStarted();

        var props = channel!.CreateBasicProperties();
        props.ContentType = "application/json";

        var body = JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions);
        channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: props, body: body);

        return Task.CompletedTask;
    }

    public async Task<TResponse> RequestAsync<TRequest, TResponse>(string queue, TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        EnsureStarted();

        var corrId = Guid.NewGuid().ToString();
        var tcs = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!pendingRequests.TryAdd(corrId, tcs))
        {
            throw new InvalidOperationException("Unable to add pending request.");
        }

        var props = channel!.CreateBasicProperties();
        props.ReplyTo = replyQueueName;
        props.CorrelationId = corrId;
        props.ContentType = "application/json";

        var body = JsonSerializer.SerializeToUtf8Bytes(request, jsonOptions);
        channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: props, body: body);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            using (cts.Token.Register(() => pendingRequests.TryRemove(corrId, out var _)))
            {
                var resultBytes = await tcs.Task.WaitAsync(cts.Token);
                var result = JsonSerializer.Deserialize<TResponse>(resultBytes, jsonOptions)!;

                return result;
            }
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException($"Timeout waiting for response from queue '{queue}'.");
        }
        finally
        {
            pendingRequests.TryRemove(corrId, out _);
        }
    }

    /// <summary>
    /// Registers a responder handler for a queue. Implements a minimal retry + DLQ mechanism.
    /// - retries: uses header "x-retry-count" and Settings.RetryCount / RetryInterval
    /// - DLQ: when retries exhausted message is published to queue + ".dlq"
    /// </summary>
    public void RegisterResponder<TRequest, TResponse>(string queue, Func<TRequest, Task<TResponse>> handler)
    {
        EnsureStarted();

        // declare main queue and DLQ
        channel!.QueueDeclare(queue: queue, durable: Settings.Durable, exclusive: false, autoDelete: Settings.AutoDelete);

        var dlq = queue + ".dlq";
        channel.QueueDeclare(queue: dlq, durable: true, exclusive: false, autoDelete: false);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var request = JsonSerializer.Deserialize<TRequest>(body, jsonOptions)!;

                var response = await handler(request);
                var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response, jsonOptions);

                var replyProps = channel!.CreateBasicProperties();
                replyProps.CorrelationId = ea.BasicProperties?.CorrelationId;
                replyProps.ContentType = "application/json";

                var replyTo = ea.BasicProperties?.ReplyTo;

                if (!string.IsNullOrEmpty(replyTo))
                {
                    channel.BasicPublish(exchange: "", routingKey: replyTo, basicProperties: replyProps, body: responseBytes);
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                // Retry/DLQ logic
                try
                {
                    var headers = ea.BasicProperties?.Headers;
                    var currentRetry = 0;

                    if (headers != null && headers.TryGetValue("x-retry-count", out var obj))
                    {
                        if (obj is byte[] bytes)
                        {
                            var s = System.Text.Encoding.UTF8.GetString(bytes);
                            int.TryParse(s, out currentRetry);
                        }
                        else if (obj is int i)
                        {
                            currentRetry = i;
                        }
                    }

                    if (currentRetry < Settings.RetryCount)
                    {
                        var newRetry = currentRetry + 1;
                        // republish with incremented header after delay
                        var republishProps = channel!.CreateBasicProperties();
                        republishProps.ContentType = ea.BasicProperties?.ContentType ?? "application/json";
                        republishProps.Headers ??= new Dictionary<string, object>();
                        republishProps.Headers["x-retry-count"] = System.Text.Encoding.UTF8.GetBytes(newRetry.ToString());
                        republishProps.Persistent = true;

                        // ack original and republish after a delay to avoid tight loops
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                        // asynchronous delay and republish (fire-and-forget)
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await Task.Delay(Settings.RetryInterval);
                                channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: republishProps, body: ea.Body.ToArray());
                            }
                            catch { /* log if needed */ }
                        });
                    }
                    else
                    {
                        // send to DLQ with error info
                        var dlqProps = channel!.CreateBasicProperties();
                        dlqProps.ContentType = ea.BasicProperties?.ContentType ?? "application/json";
                        dlqProps.Headers ??= new Dictionary<string, object>();
                        dlqProps.Headers["x-error"] = System.Text.Encoding.UTF8.GetBytes(ex.Message ?? "error");
                        dlqProps.Persistent = true;

                        // publish original body into dlq
                        channel.BasicPublish(exchange: "", routingKey: dlq, basicProperties: dlqProps, body: ea.Body.ToArray());

                        // ack original to remove it from main queue
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                }
                catch
                {
                    // If DLQ or republish also fails, attempt to nack without requeue
                    try
                    {
                        channel!.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                    }
                    catch { }
                }
            }
        };

        channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
    }

    private void EnsureStarted()
    {
        if (!started)
        {
            throw new InvalidOperationException("Bus not started. Call Start() before using the bus.");
        }

        if (channel == null)
        {
            throw new InvalidOperationException("Channel not available.");
        }
    }

    public ValueTask DisposeAsync()
    {
        Stop();
        return ValueTask.CompletedTask;
    }
}