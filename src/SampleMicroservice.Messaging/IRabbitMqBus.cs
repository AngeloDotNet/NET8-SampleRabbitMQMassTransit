namespace SampleMicroservice.Messaging;

public interface IRabbitMqBus : IAsyncDisposable
{
    Task PublishAsync<T>(string routingKey, T message, CancellationToken cancellationToken = default);
    Task<TResponse> RequestAsync<TRequest, TResponse>(string queue, TRequest request, TimeSpan timeout, CancellationToken cancellationToken = default);
    void RegisterResponder<TRequest, TResponse>(string queue, Func<TRequest, Task<TResponse>> handler);
    void Start();
    void Stop();
}