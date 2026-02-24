namespace SampleMicroservice.Messaging;

/// <summary>
/// Represents an abstraction for interacting with a RabbitMQ-backed message bus.
/// Provides publish/subscribe, request/response and lifecycle operations required by the application.
/// </summary>
public interface IRabbitMqBus : IAsyncDisposable
{
    /// <summary>
    /// Publishes a message to the message broker using the specified routing key.
    /// </summary>
    /// <typeparam name="T">The runtime type of the message being published.</typeparam>
    /// <param name="routingKey">The routing key or topic used by the broker to route the message.</param>
    /// <param name="message">The message payload to publish.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the publish operation to complete.</param>
    /// <returns>A task that represents the asynchronous publish operation.</returns>
    /// <exception cref="OperationCanceledException">If the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request message to the specified queue and waits for a response of the specified type.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <typeparam name="TResponse">The expected response type.</typeparam>
    /// <param name="queue">The destination queue name to which the request is sent.</param>
    /// <param name="request">The request message payload.</param>
    /// <param name="timeout">The maximum time to wait for a response before the request times out.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the response.</param>
    /// <returns>
    /// A task that completes with the response message of type <typeparamref name="TResponse"/>.
    /// </returns>
    /// <exception cref="TimeoutException">If a response is not received within <paramref name="timeout"/>.</exception>
    /// <exception cref="OperationCanceledException">If the operation is cancelled via <paramref name="cancellationToken"/>.</exception>
    Task<TResponse> RequestAsync<TRequest, TResponse>(string queue, TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a responder handler for incoming request messages received on the specified queue.
    /// The handler will be invoked for each incoming request and its result will be sent back as the response.
    /// </summary>
    /// <typeparam name="TRequest">The type of the incoming request message.</typeparam>
    /// <typeparam name="TResponse">The type of the response message the handler returns.</typeparam>
    /// <param name="queue">The queue name on which to listen for requests.</param>
    /// <param name="handler">
    /// A function that receives a <typeparamref name="TRequest"/> and returns a <typeparamref name="TResponse"/> asynchronously.
    /// </param>
    /// <remarks>
    /// Implementations should ensure the handler is executed safely and consider concurrency and exception handling
    /// for each incoming request.
    /// </remarks>
    void RegisterResponder<TRequest, TResponse>(string queue, Func<TRequest, Task<TResponse>> handler);

    /// <summary>
    /// Starts the bus, establishing required connections and beginning message processing.
    /// </summary>
    /// <remarks>
    /// This should be called once during application startup before publish/request/responder operations are used.
    /// </remarks>
    void Start();

    /// <summary>
    /// Stops the bus, terminating connections and ceasing message processing.
    /// </summary>
    /// <remarks>
    /// After calling <see cref="Stop"/>, the bus may need to be started again with <see cref="Start"/> before use.
    /// </remarks>
    void Stop();
}