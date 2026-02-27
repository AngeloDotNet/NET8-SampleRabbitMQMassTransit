using ClassLibrary.EFCore;
using ClassLibrary.EFCore.Interfaces;
using SampleMicroservice.Messaging;
using WebAPI.Backend.Core.Handlers;
using WebAPI.Backend.HostedServices;

namespace WebAPI.Backend.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddDatabaseServices(this IServiceCollection services)
    {
        // Register DbContext with in-memory database
        services.AddDbContext<DataDbContext>(option => { option.UseInMemoryDatabase(Settings.DatabaseName); });

        // Register repositories
        services.AddScoped<DbContext, DataDbContext>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        //services.AddScoped<IRepository<PersonEntity, int>, Repository<PersonEntity, int>>();

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddTransient<IPeopleService, PeopleService>();

        // Register handlers (transient so they can use scoped dependencies like DbContext / IPeopleService)
        services.AddTransient<PeopleListRequestHandler>();
        services.AddTransient<PersonRequestHandler>();

        return services;
    }

    public static IServiceCollection AddRabbitMqBus(this IServiceCollection services, string host, string username, string password, string virtualHost)
    {
        // Register bus singleton using Settings values and resolve ILogger<RabbitMqBus> from DI (dependency injection)
        services.AddSingleton<IRabbitMqBus>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<RabbitMqBus>>();
            return new RabbitMqBus(host, username, password, virtualHost, logger);
        });

        // Hosted service that starts the bus and registers responders
        services.AddHostedService<RabbitResponderHostedService>();

        return services;
    }
}