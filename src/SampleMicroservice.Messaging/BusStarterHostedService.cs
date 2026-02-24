using Microsoft.Extensions.Hosting;

namespace SampleMicroservice.Messaging;

/// <summary>
/// Hosted service that starts and stops the RabbitMQ bus for the application's lifetime.
/// </summary>
/// <param name="bus">The RabbitMQ bus used to start and stop message processing.</param>
/// <remarks>
/// This hosted service performs synchronous start/stop operations by calling
/// the bus's <c>Start</c> and <c>Stop</c> methods and returns a completed task
/// to satisfy the <see cref="IHostedService"/> contract.
/// </remarks>
public class BusStarterHostedService(IRabbitMqBus bus) : IHostedService
{
    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the start operation.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous start operation. This implementation
    /// performs a synchronous start and returns <see cref="Task.CompletedTask"/>.
    /// </returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        bus.Start();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the stop operation.</param>
    /// <returns>
    /// A <see cref="Task"/> that represents the asynchronous stop operation. This implementation
    /// performs a synchronous stop and returns <see cref="Task.CompletedTask"/>.
    /// </returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        bus.Stop();

        return Task.CompletedTask;
    }
}