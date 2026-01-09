using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleMicroservice.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqBus(this IServiceCollection services, string host, string username, string password, string virtualHost)
    {
        //services.AddSingleton<IRabbitMqBus>(sp => new RabbitMqBus(host, username, password, virtualHost, logger));

        services.AddSingleton<IRabbitMqBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqBus>>();

            return new RabbitMqBus(host, username, password, virtualHost, logger);
        });

        //services.AddSingleton<RabbitMqBus>(sp =>
        //{
        //    var logger = sp.GetRequiredService<ILogger<RabbitMqBus>>();
        //    return new RabbitMqBus(host, username, password, virtualHost, logger);
        //});
        //services.AddSingleton<IRabbitMqBus>(sp => sp.GetRequiredService<RabbitMqBus>());

        return services;
    }
}