using WebAPI.Frontend.Extensions;

namespace WebAPI.Frontend;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddLogging();

        // Register Swagger
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // Register bus singleton with Settings values
        services.AddFrontEndRabbitMQ(Settings.RabbitMQHost, Settings.RabbitMQUsername, Settings.RabbitMQPassword, Settings.RabbitMQVirtualHost);

        // Add hosted service to start/stop the bus automatically
        //services.AddHostedService<BusStarterHostedService>();
    }

    public void Configure(WebApplication app)
    {
        var env = app.Environment;

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.MapControllers();
    }
}