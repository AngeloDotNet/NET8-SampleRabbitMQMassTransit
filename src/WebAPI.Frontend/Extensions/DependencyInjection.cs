namespace WebAPI.Frontend.Extensions;

public static class DependencyInjection
{
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
            //x.AddRequestClient<PeopleListRequest>();
            //x.AddRequestClient<PersonRequest>();
        });

        return services;
    }
}