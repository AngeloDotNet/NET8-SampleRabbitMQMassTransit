using ClassLibrary.EFCore;
using ClassLibrary.EFCore.Interfaces;
using WebAPI.Backend.Extensions;

namespace WebAPI.Backend;

/// <summary>
/// Startup class for the application.
/// </summary>
public class Startup
{
    /// <summary>
    /// Gets the configuration of the application.
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Startup"/> class.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>
    /// Configures the services for the application.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();
        services.AddDbContext<DataDbContext>(option => { option.UseInMemoryDatabase(Settings.DatabaseName); });

        services
            .AddScoped<DbContext, DataDbContext>()
            .AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        //services.AddScoped<IRepository<PersonEntity, int>, Repository<PersonEntity, int>>();

        services.AddTransient<IPeopleService, PeopleService>();
        services.AddBackEndRabbitMQ();
    }

    /// <summary>
    /// Configures the application.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public void Configure(WebApplication app)
    {
        IWebHostEnvironment env = app.Environment;

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.AddDataPeopleDemo();
        app.UseRouting();

        app.MapControllers();
    }
}
