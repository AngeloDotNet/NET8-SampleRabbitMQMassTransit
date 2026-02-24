using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SampleMicroservice.Shared;

namespace SampleMicroservice.Messaging;

/// <summary>
/// A lightweight RabbitMQ bus used by the sample microservice.
/// </summary>
/// <remarks>
/// Responsibilities:
/// - manage a single connection and channel to RabbitMQ,
/// - provide simple publish (fire-and-forget) functionality,
/// - provide RPC-style request/response via temporary reply queue,
/// - register responders that process incoming messages with retry/DLQ semantics.
/// 
/// This class is not thread-safe for concurrent Start/Stop calls. Normal usage is:
/// - Create instance,
/// - Call <see cref="Start"/> once,
/// - Use <see cref="PublishAsync{T}"/>, <see cref="RequestAsync{TRequest,TResponse}"/>, or <see cref="RegisterResponder{TRequest,TResponse}"/>.
/// - Call <see cref="Stop"/> or <see cref="DisposeAsync"/> on shutdown.
/// </remarks>
public class RabbitMqBus : IRabbitMqBus
{
    private readonly ConnectionFactory factory;
    private readonly ILogger<RabbitMqBus>? logger;
    private IConnection? connection;
    private IModel? channel;

    private string? replyQueueName;
    private EventingBasicConsumer? replyConsumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<byte[]>> pendingRequests = new();

    private readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web);
    private bool started;

    /// <summary>
    /// Initializes a new instance of the <see cref="RabbitMqBus"/> class.
    /// </summary>
    /// <param name="hostName">The RabbitMQ host name or address.</param>
    /// <param name="userName">The user name used to authenticate with RabbitMQ.</param>
    /// <param name="password">The password used to authenticate with RabbitMQ.</param>
    /// <param name="virtualHost">The RabbitMQ virtual host to use.</param>
    /// <param name="logger">An optional logger instance for diagnostic messages.</param>
    public RabbitMqBus(string hostName, string userName, string password, string virtualHost, ILogger<RabbitMqBus> logger)
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

        this.logger = logger;
    }

    /// <summary>
    /// Starts the bus by creating a connection and a channel, sets basic QoS, and declares a temporary reply queue for RPC responses.
    /// </summary>
    /// <remarks>
    /// Calling <see cref="Start"/> more than once is a no-op. When started the instance:
    /// - Creates a connection and channel,
    /// - Declares an exclusive, auto-delete reply queue and a consumer to accept RPC replies,
    /// - Configures a QoS prefetch count from <c>Settings.PrefetchCount</c> (best-effort).
    /// </remarks>
    public void Start()
    {
        if (started)
        {
            return;
        }

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        try
        {
            channel.BasicQos(0, (ushort)Settings.PrefetchCount, false);
        }
        catch
        { }

        var replyQueue = channel.QueueDeclare(queue: "", durable: false, exclusive: true, autoDelete: true);
        replyQueueName = replyQueue.QueueName;

        replyConsumer = new EventingBasicConsumer(channel);
        replyConsumer.Received += (sender, ea) =>
        {
            try
            {
                var corr = ea.BasicProperties?.CorrelationId;

                if (!string.IsNullOrEmpty(corr) && pendingRequests.TryRemove(corr, out var tcs))
                {
                    logger?.LogDebug("RPC: received response for CorrelationId={CorrelationId}", corr);
                    tcs.TrySetResult(ea.Body.ToArray());
                }
                else
                {
                    logger?.LogDebug("RPC: received response with unknown CorrelationId={CorrelationId}", corr);
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error processing RPC reply");
            }
        };
        channel.BasicConsume(queue: replyQueueName, autoAck: true, consumer: replyConsumer);

        started = true;
        logger?.LogInformation("RabbitMqBus started (replyQueue={ReplyQueue})", replyQueueName);
    }

    /// <summary>
    /// Stops the bus and releases all RabbitMQ resources.
    /// </summary>
    /// <remarks>
    /// This method attempts to close channel and connection, logs any warning if closing fails,
    /// and then disposes resources and clears internal state (including pending RPC requests).
    /// </remarks>
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
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "Error while closing RabbitMQ connection/channel");
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
            logger?.LogInformation("RabbitMqBus stopped");
        }
    }

    /// <summary>
    /// Publishes a message to the specified routing key (queue) as JSON.
    /// </summary>
    /// <typeparam name="T">Type of the message to publish. Will be serialized to JSON using <see cref="System.Text.Json"/> with web defaults.</typeparam>
    /// <param name="routingKey">The routing key or queue name to publish the message to.</param>
    /// <param name="message">The message object to publish.</param>
    /// <param name="cancellationToken">A cancellation token (currently unused by the implementation).</param>
    /// <returns>A completed task once the message has been published locally.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the bus has not been started or the channel is not available.</exception>
    public Task PublishAsync<T>(string routingKey, T message, System.Threading.CancellationToken cancellationToken = default)
    {
        EnsureStarted();

        var props = channel!.CreateBasicProperties();
        props.ContentType = "application/json";

        var body = JsonSerializer.SerializeToUtf8Bytes(message, jsonOptions);
        channel.BasicPublish(exchange: "", routingKey: routingKey, basicProperties: props, body: body);

        logger?.LogDebug("Published message to routingKey={RoutingKey}", routingKey);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sends a request message to a queue and waits for a response using a temporary reply queue (RPC pattern).
    /// </summary>
    /// <typeparam name="TRequest">Request message type that will be serialized to JSON.</typeparam>
    /// <typeparam name="TResponse">Expected response type that will be deserialized from JSON.</typeparam>
    /// <param name="queue">The target queue to which the request will be sent.</param>
    /// <param name="request">The request payload.</param>
    /// <param name="timeout">Maximum time to wait for a response before throwing <see cref="TimeoutException"/>.</param>
    /// <param name="cancellationToken">Cancellation token that can be used to cancel the wait.</param>
    /// <returns>The deserialized response of type <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the bus has not been started or the channel is not available.</exception>
    /// <exception cref="TimeoutException">Thrown when the timeout elapses while waiting for a response.</exception>
    public async Task<TResponse> RequestAsync<TRequest, TResponse>(string queue, TRequest request, TimeSpan timeout, System.Threading.CancellationToken cancellationToken = default)
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
        logger?.LogDebug("RPC: publishing request to queue={Queue} CorrelationId={CorrelationId}", queue, corrId);
        channel.BasicPublish(exchange: "", routingKey: queue, basicProperties: props, body: body);

        using var cts = System.Threading.CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout);

        try
        {
            using (cts.Token.Register(() => pendingRequests.TryRemove(corrId, out var _)))
            {
                var resultBytes = await tcs.Task.WaitAsync(cts.Token);
                var result = JsonSerializer.Deserialize<TResponse>(resultBytes, jsonOptions)!;
                logger?.LogDebug("RPC: response deserialized CorrelationId={CorrelationId}", corrId);
                return result;
            }
        }
        catch (OperationCanceledException)
        {
            logger?.LogWarning("RPC: timeout waiting for response from queue={Queue} CorrelationId={CorrelationId}", queue, corrId);
            throw new TimeoutException($"Timeout waiting for response from queue '{queue}'.");
        }
        finally
        {
            pendingRequests.TryRemove(corrId, out _);
        }
    }

    /// <summary>
    /// Registers an asynchronous responder for messages arriving on the specified queue.
    /// </summary>
    /// <typeparam name="TRequest">Type to which incoming messages will be deserialized.</typeparam>
    /// <typeparam name="TResponse">Type returned by the handler and sent back to the requester (if a ReplyTo is present).</typeparam>
    /// <param name="queue">Queue name to consume messages from.</param>
    /// <param name="handler">
    /// Asynchronous handler invoked for each deserialized request. The handler should not assume exclusive access to the thread,
    /// may be awaited concurrently for multiple messages, and should throw on processing errors so retry/DLQ logic can be applied.
    /// </param>
    /// <remarks>
    /// Behavior:
    /// - Declares the main queue and a DLQ named "{queue}.dlq".
    /// - Declares a set of retry queues named "{queue}.retry.{N}" using exponentially increasing TTLs (based on <c>Settings.RetryInterval</c>).
    /// - When handler succeeds, an RPC response is published to the incoming message's ReplyTo (if present) and the message is ACKed.
    /// - On handler exception the message is either re-published to the next retry queue (with an incremented "x-retry-count" header) or moved to DLQ after the configured retry attempts.
    /// - The method sets <see cref="MessageContext.CorrelationId"/> for the duration of handler invocation and ensures it is cleared afterwards.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown if the bus has not been started or the channel is not available.</exception>
    public void RegisterResponder<TRequest, TResponse>(string queue, Func<TRequest, Task<TResponse>> handler)
    {
        EnsureStarted();

        if (channel == null)
        {
            throw new InvalidOperationException("Channel not available.");
        }

        logger?.LogInformation("Registering responder for queue={Queue}", queue);

        channel.QueueDeclare(queue: queue, durable: Settings.Durable, exclusive: false, autoDelete: Settings.AutoDelete);
        var dlq = queue + ".dlq";
        channel.QueueDeclare(queue: dlq, durable: true, exclusive: false, autoDelete: false);

        var retryQueues = new List<string>();
        for (var attempt = 1; attempt <= Settings.RetryCount; attempt++)
        {
            var retryQueueName = $"{queue}.retry.{attempt}";
            var ttl = Settings.RetryInterval * (1 << (attempt - 1));

            var retryArgs = new Dictionary<string, object?>
                {
                    { "x-dead-letter-exchange", "" },
                    { "x-dead-letter-routing-key", queue },
                    { "x-message-ttl", ttl }
                };

            channel.QueueDeclare(queue: retryQueueName, durable: true, exclusive: false, autoDelete: false, arguments: retryArgs);
            retryQueues.Add(retryQueueName);

            logger?.LogDebug("Declared retry queue={RetryQueue} ttlMs={TTL}", retryQueueName, ttl);
        }

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.Received += async (sender, ea) =>
        {
            var deliveryTag = ea.DeliveryTag;
            // estraggo correlationId e imposto il contesto per l'handler
            var corrId = ea.BasicProperties?.CorrelationId;
            try
            {
                var body = ea.Body.ToArray();
                var msgHeaders = ea.BasicProperties?.Headers;
                var currentRetry = 0;
                if (msgHeaders != null && msgHeaders.TryGetValue("x-retry-count", out var obj))
                {
                    if (obj is byte[] bytes)
                    {
                        var s = Encoding.UTF8.GetString(bytes);
                        int.TryParse(s, out currentRetry);
                    }
                    else if (obj is int i)
                    {
                        currentRetry = i;
                    }
                }

                logger?.LogDebug("Received message on queue={Queue} deliveryTag={DeliveryTag} correlationId={CorrelationId} currentRetry={CurrentRetry}", queue, deliveryTag, corrId, currentRetry);

                var request = JsonSerializer.Deserialize<TRequest>(body, jsonOptions)!;

                // setto contesto (propaga tramite AsyncLocal ai handler chiamati)
                MessageContext.CorrelationId = corrId;

                TResponse response;
                try
                {
                    response = await handler(request);
                }
                finally
                {
                    // garantisco la pulizia del contesto indipendentemente dall'esito
                    MessageContext.Clear();
                }

                var responseBytes = JsonSerializer.SerializeToUtf8Bytes(response, jsonOptions);

                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = corrId;
                replyProps.ContentType = "application/json";

                var replyTo = ea.BasicProperties?.ReplyTo;
                if (!string.IsNullOrEmpty(replyTo))
                {
                    channel.BasicPublish(exchange: "", routingKey: replyTo, basicProperties: replyProps, body: responseBytes);
                    logger?.LogDebug("Published RPC response to ReplyTo={ReplyTo} CorrelationId={CorrelationId}", replyTo, corrId);
                }
                else
                {
                    logger?.LogWarning("No ReplyTo found for incoming RPC request; CorrelationId={CorrelationId}", corrId);
                }

                channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error handling message from queue={Queue} correlationId={CorrelationId}", queue, corrId);

                try
                {
                    var headers = ea.BasicProperties?.Headers;
                    var currentRetry = 0;
                    if (headers != null && headers.TryGetValue("x-retry-count", out var obj))
                    {
                        if (obj is byte[] bytes)
                        {
                            var s = Encoding.UTF8.GetString(bytes);
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
                        var retryQueueName = $"{queue}.retry.{newRetry}";

                        var republishProps = channel.CreateBasicProperties();
                        republishProps.ContentType = ea.BasicProperties?.ContentType ?? "application/json";
                        republishProps.Headers ??= new Dictionary<string, object>();
                        republishProps.Headers["x-retry-count"] = Encoding.UTF8.GetBytes(newRetry.ToString());
                        republishProps.Persistent = true;

                        channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
                        channel.BasicPublish(exchange: "", routingKey: retryQueueName, basicProperties: republishProps, body: ea.Body.ToArray());

                        logger?.LogWarning("Message from queue={Queue} scheduled for retry #{Retry} via queue={RetryQueue} correlationId={CorrelationId}", queue, newRetry, retryQueueName, corrId);
                    }
                    else
                    {
                        var dlqProps = channel.CreateBasicProperties();
                        dlqProps.ContentType = ea.BasicProperties?.ContentType ?? "application/json";
                        dlqProps.Headers ??= new Dictionary<string, object>();
                        dlqProps.Headers["x-error"] = Encoding.UTF8.GetBytes(ex.Message ?? "error");
                        dlqProps.Persistent = true;

                        channel.BasicPublish(exchange: "", routingKey: dlq, basicProperties: dlqProps, body: ea.Body.ToArray());
                        channel.BasicAck(deliveryTag: deliveryTag, multiple: false);

                        logger?.LogWarning("Message from queue={Queue} moved to DLQ={DLQ} after {Retries} attempts correlationId={CorrelationId}", queue, dlq, currentRetry, corrId);
                    }
                }
                catch (Exception innerEx)
                {
                    logger?.LogError(innerEx, "Failed while handling retry/DLQ for message from queue={Queue} correlationId={CorrelationId}", queue, corrId);
                    try
                    {
                        channel.BasicNack(deliveryTag: deliveryTag, multiple: false, requeue: false);
                    }
                    catch { /* best-effort */ }
                }
            }
        };

        channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
        logger?.LogInformation("Responder registered for queue={Queue} (DLQ={DLQ}, retryCount={RetryCount})", queue, dlq, Settings.RetryCount);
    }

    /// <summary>
    /// Ensures the bus has been started and a channel is available.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the bus was not started or the channel is not available.</exception>
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

    /// <summary>
    /// Performs an asynchronous disposal by stopping the bus and releasing resources.
    /// </summary>
    /// <returns>A completed <see cref="ValueTask"/> once disposal is finished.</returns>
    public ValueTask DisposeAsync()
    {
        Stop();
        return ValueTask.CompletedTask;
    }
}