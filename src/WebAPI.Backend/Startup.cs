using WebAPI.Backend.Extensions;

namespace WebAPI.Backend;

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

        // Register database services
        services.AddDatabaseServices();

        // Register bus singleton with Settings values
        services.AddRabbitMqBus(Settings.RabbitMQHost, Settings.RabbitMQUsername, Settings.RabbitMQPassword, Settings.RabbitMQVirtualHost);

        // Register application services
        services.AddApplicationServices();
    }

    public void Configure(WebApplication app)
    {
        var env = app.Environment;

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Seed in-memory database
        SeedDatabase(app);

        app.UseRouting();
        app.MapControllers();
    }

    public static void SeedDatabase(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<DataDbContext>();
        var listPerson = new List<PersonEntity>();

        db.ChangeTracker.Clear();

        for (var i = 1; i <= 10; i++)
        {
            var person = new PersonEntity
            {
                Id = i,
                UserId = Guid.NewGuid(),
                Cognome = $"Cognome{i}",
                Nome = $"Nome{i}",
                Email = string.Concat($"C{i}", ".", $"Nome{i}", "@example.com")
            };

            listPerson.Add(person);
        }

        db.People.AddRange(listPerson);
        db.SaveChanges();
    }
}