namespace WebAPI.Backend.Infrastructure;

public class DataDbContext(DbContextOptions<DataDbContext> options) : DbContext(options)
{
    public virtual DbSet<PersonEntity> People { get; set; }

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
