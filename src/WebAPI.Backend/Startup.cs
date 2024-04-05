using ClassLibrary.EFCore;
using ClassLibrary.EFCore.Interfaces;
using WebAPI.Backend.Extensions;

namespace WebAPI.Backend;

public class Startup(IConfiguration configuration)
{
    public IConfiguration Configuration { get; } = configuration;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen();
        services.AddDbContext<DataDbContext>(option => { option.UseInMemoryDatabase(Settings.DatabaseName); });

        services.AddScoped<DbContext, DataDbContext>();
        //services.AddScoped<IRepository<PersonEntity, int>, Repository<PersonEntity, int>>();
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        services.AddTransient<IPeopleService, PeopleService>();
        services.AddBackEndRabbitMQ();
    }

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