namespace WebAPI.Backend.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Adds the backend RabbitMQ service to the specified IServiceCollection.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the service to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    public static IServiceCollection AddBackEndRabbitMQ(this IServiceCollection services)
    {
        services.AddMassTransit(x =>
        {
            x.AddConsumers(typeof(ConsumerPersonListRequest).Assembly);

            x.SetKebabCaseEndpointNameFormatter();
            x.UsingRabbitMq((context, rabbit) =>
            {
                rabbit.Host(Settings.RabbitMQHost, Settings.RabbitMQVirtualHost, h =>
                {
                    h.Username(Settings.RabbitMQUsername);
                    h.Password(Settings.RabbitMQPassword);
                });

                rabbit.ReceiveEndpoint(Settings.QueueNameResponse, e =>
                {
                    e.Durable = Settings.Durable;
                    e.AutoDelete = Settings.AutoDelete;
                    e.ExchangeType = Settings.ExchangeType;
                    e.PrefetchCount = Settings.PrefetchCount;

                    e.ConfigureConsumers(context);
                });
            });
        });

        //Registrazione service bus aggiuntivi
        //services.AddBackEndRabbitMQSecondBus();

        return services;
    }

    //public static IServiceCollection AddBackEndRabbitMQSecondBus(this IServiceCollection services)
    //{
    //    services.AddMassTransit<ISecondBus>(x =>
    //    {
    //        //x.AddConsumer<ConsumerPersonRequest>();
    //        x.AddConsumers(typeof(ConsumerPersonRequest).Assembly);
    //        //x.AddConsumers(typeof(ConsumerPersonListRequest).Assembly);

    //        x.SetKebabCaseEndpointNameFormatter();
    //        x.UsingRabbitMq((context, rabbit) =>
    //        {
    //            rabbit.Host(Settings.RabbitMQHost, Settings.RabbitMQVirtualHost, h =>
    //            {
    //                h.Username(Settings.RabbitMQUsername);
    //                h.Password(Settings.RabbitMQPassword);
    //            });

    //            rabbit.ReceiveEndpoint("responsePeople2", e =>
    //            {
    //                e.Durable = Settings.Durable;
    //                e.AutoDelete = Settings.AutoDelete;
    //                e.ExchangeType = Settings.ExchangeType;
    //                e.PrefetchCount = Settings.PrefetchCount;

    //                //e.ConfigureConsumer<ConsumerPersonRequest>(context);
    //                e.ConfigureConsumers(context);
    //            });
    //        });
    //    });

    //    return services;
    //}
}