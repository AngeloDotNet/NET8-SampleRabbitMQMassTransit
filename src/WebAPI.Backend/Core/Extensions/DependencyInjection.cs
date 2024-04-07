namespace WebAPI.Backend.Core.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Adds demo data to the People table in the database.
    /// </summary>
    /// <param name="app">The web application to which the data is added.</param>
    /// <returns>The same web application after adding the data.</returns>
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
