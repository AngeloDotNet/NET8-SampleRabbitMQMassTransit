using SampleMicroservice.Messaging;

namespace WebAPI.Frontend.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddFrontEndRabbitMQ(this IServiceCollection services, string host, string username, string password, string virtualHost)
    {
        // Register bus singleton using Settings values and resolve ILogger<RabbitMqBus> from DI
        services.AddSingleton<IRabbitMqBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqBus>>();
            return new RabbitMqBus(host, username, password, virtualHost, logger);
        });

        // add hosted service to start/stop the bus automatically
        services.AddHostedService<BusStarterHostedService>();

        return services;
    }
}