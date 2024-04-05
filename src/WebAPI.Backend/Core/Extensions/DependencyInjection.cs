namespace WebAPI.Backend.Core.Extensions;

public static class DependencyInjection
{
    public static WebApplication AddDataPeopleDemo(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<DataDbContext>();
        var listPerson = new List<PersonEntity>();

        db.ChangeTracker.Clear();

        for (var i = 1; i <= 10; i++)
        {
            var person = new PersonEntity { Id = i, UserId = Guid.NewGuid(), Cognome = $"Cognome{i}", Nome = $"Nome{i}", Email = string.Concat($"C{i}", ".", $"Nome{i}", "@example.com") };

            listPerson.Add(person);
        }

        db.People.AddRange(listPerson);
        db.SaveChanges();

        return app;
    }
}