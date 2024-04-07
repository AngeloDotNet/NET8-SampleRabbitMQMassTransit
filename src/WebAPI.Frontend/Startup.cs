using WebAPI.Frontend.Extensions;

namespace WebAPI.Frontend;

/// <summary>
/// Startup class for the application.
/// </summary>
/// <param name="configuration">The application configuration.</param>
public class Startup(IConfiguration configuration)
{
    /// <summary>
    /// Gets the application configuration.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration;

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();
        services.AddFrontEndRabbitMQ(Settings.RabbitMQVirtualHost, Settings.QueueNameRequest);
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">The web application.</param>
    public void Configure(WebApplication app)
    {
        IWebHostEnvironment env = app.Environment;

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();
        app.MapControllers();
    }
}
