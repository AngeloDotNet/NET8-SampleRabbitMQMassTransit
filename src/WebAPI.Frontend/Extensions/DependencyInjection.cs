namespace WebAPI.Frontend.Extensions;

/// <summary>
/// Contains extension methods for dependency injection.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds RabbitMQ as a message broker using MassTransit.
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to.</param>
    /// <param name="vHost">The virtual host to connect to on the RabbitMQ server.</param>
    /// <param name="queueName">The name of the queue to use.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddFrontEndRabbitMQ(this IServiceCollection services, string vHost, string queueName)
    {
        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.QueueExpiration = TimeSpan.FromSeconds(Settings.QueueExpiration);
                rabbit.Host(Settings.RabbitMQHost, vHost, h =>
                {
                    h.Username(Settings.RabbitMQUsername);
                    h.Password(Settings.RabbitMQPassword);
                });

                rabbit.ReceiveEndpoint(queueName, e =>
                {
                    e.Durable = Settings.Durable;
                    e.AutoDelete = Settings.AutoDelete;
                    e.ExchangeType = Settings.ExchangeType;
                    e.PrefetchCount = Settings.PrefetchCount;

                    e.UseMessageRetry(r => r.Interval(Settings.RetryCount, Settings.RetryInterval));
                });
            });

            x.AddRequestClient(typeof(PersonRequest));
        });

        return services;
    }
}
