using Microsoft.Extensions.Hosting;

namespace SampleMicroservice.Messaging;

public class BusStarterHostedService(IRabbitMqBus bus) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        bus.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        bus.Stop();
        return Task.CompletedTask;
    }
}