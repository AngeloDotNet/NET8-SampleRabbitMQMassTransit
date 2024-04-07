namespace WebAPI.Backend.Infrastructure;

/// <summary>
/// The DataDbContext class is a representation of the database context used in the application.
/// It inherits from the DbContext class provided by Entity Framework Core.
/// </summary>
/// <param name="options">The options to be used by a DbContext.</param>
public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets or sets the DbSet of PersonEntity.
    /// </summary>
    public virtual DbSet<PersonEntity> People { get; set; }

    /// <summary>
    /// Configures the model that was discovered by convention from the entity types
    /// exposed in DbSet properties on your derived context. 
    /// The resulting model may be cached and re-used for subsequent instances of your derived context.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context. Databases (and other extensions) typically
    /// define extension methods on this object that allow you to configure aspects of the model that are specific to a given database.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PersonEntity>(entity =>
        {
            //entity.ToTable("People");
            entity.HasKey(e => e.Id);
        });
    }
}
