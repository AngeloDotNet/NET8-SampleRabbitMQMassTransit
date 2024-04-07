namespace WebAPI.Backend;

/// <summary>
/// The main entry point for the application.
/// </summary>
/// <remarks>
/// This class configures the web application and starts it.
/// </remarks>
public class Program
{
    /// <summary>
    /// The main method for the application.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
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
