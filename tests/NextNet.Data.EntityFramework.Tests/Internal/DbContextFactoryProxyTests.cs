using NextNet.Data.EntityFramework.Internal;

namespace NextNet.Data.EntityFramework.Tests.Internal;

/// <summary>
/// Tests for <see cref="DbContextFactoryProxy"/>.
/// </summary>
public sealed class DbContextFactoryProxyTests
{
    [Fact]
    public void CreateDbContext_Should_ReturnContext()
    {
        // Arrange
        var innerFactory = new InMemoryTestDbContextFactory();
        var options = new EfCoreOptions();
        var proxy = new DbContextFactoryProxy(innerFactory, options);

        // Act
        var context = proxy.CreateDbContext();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<AppDbContext>(context);
    }

    [Fact]
    public async Task CreateDbContextAsync_Should_ReturnContext()
    {
        // Arrange
        var innerFactory = new InMemoryTestDbContextFactory();
        var options = new EfCoreOptions();
        var proxy = new DbContextFactoryProxy(innerFactory, options);

        // Act
        var context = await proxy.CreateDbContextAsync();

        // Assert
        Assert.NotNull(context);
        Assert.IsType<AppDbContext>(context);
    }

    [Fact]
    public void Constructor_Should_Throw_When_InnerFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new DbContextFactoryProxy(null!, new EfCoreOptions()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_OptionsIsNull()
    {
        var innerFactory = new InMemoryTestDbContextFactory();
        Assert.Throws<ArgumentNullException>(() =>
            new DbContextFactoryProxy(innerFactory, null!));
    }

    /// <summary>
    /// Simple factory for testing the proxy.
    /// </summary>
    private sealed class InMemoryTestDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private static int _counter;

        public AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase($"ProxyTestDb_{Interlocked.Increment(ref _counter)}")
                .Options;

            return new AppDbContext(options);
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateDbContext());
        }
    }
}
