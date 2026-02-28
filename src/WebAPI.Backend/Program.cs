namespace WebAPI.Backend;

public class Program
{
    public static void Main(string[] args)
    {
        // Create a builder for the web application
        var builder = WebApplication.CreateBuilder(args);

        // Create a new startup instance
        Startup startup = new(builder.Configuration);

        // Configure the services
        startup.ConfigureServices(builder.Services);

        // Build the application
        var app = builder.Build();

        // Configure the application
        startup.Configure(app);

        // Run the application
        app.Run();
    }
}
