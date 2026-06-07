namespace NextNet.Data.EntityFramework.Tests.Fixtures;

/// <summary>
/// Test-specific DbContext that registers <see cref="TestEntity"/> as a DbSet.
/// </summary>
public sealed class TestAppDbContext : AppDbContext
{
    /// <summary>
    /// Initializes a new instance of <see cref="TestAppDbContext"/>.
    /// </summary>
    /// <param name="options">The DbContext options.</param>
    public TestAppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets the test entities DbSet.
    /// </summary>
    public DbSet<TestEntity> TestEntities => Set<TestEntity>();

    /// <summary>
    /// Configures the entity model for tests.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Value).HasColumnType("decimal(18,2)");
        });

        base.OnModelCreating(modelBuilder);
    }
}
