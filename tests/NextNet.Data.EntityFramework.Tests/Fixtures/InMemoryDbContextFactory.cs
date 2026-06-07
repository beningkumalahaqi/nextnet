namespace NextNet.Data.EntityFramework.Tests.Fixtures;

/// <summary>
/// Creates an <see cref="IDbContextFactory{AppDbContext}"/> backed by the EF Core InMemory provider.
/// Each instance uses a unique database name for test isolation.
/// </summary>
public sealed class InMemoryDbContextFactory : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();
    private bool _disposed;

    /// <summary>
    /// Creates an <see cref="IDbContextFactory{AppDbContext}"/> using the InMemory provider
    /// that creates <see cref="TestAppDbContext"/> instances.
    /// </summary>
    /// <returns>A factory for creating DbContext instances.</returns>
    public IDbContextFactory<AppDbContext> CreateFactory()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new TestDbContextFactory(options);
    }

    /// <summary>
    /// Creates a new <see cref="TestAppDbContext"/> instance backed by InMemory database.
    /// </summary>
    /// <returns>A new DbContext instance.</returns>
    public TestAppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new TestAppDbContext(options);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }

    /// <summary>
    /// Internal factory that creates TestAppDbContext instances with InMemory options.
    /// Implements <see cref="IDbContextFactory{AppDbContext}"/> for compatibility with
    /// the repository pattern.
    /// </summary>
    private sealed class TestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public TestDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public AppDbContext CreateDbContext()
        {
            return new TestAppDbContext(_options);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AppDbContext>(new TestAppDbContext(_options));
        }
    }
}
