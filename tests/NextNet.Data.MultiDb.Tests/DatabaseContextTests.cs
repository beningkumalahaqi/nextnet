using NextNet.Data.MultiDb.Tests.Fixtures;

namespace NextNet.Data.MultiDb.Tests;

public class DatabaseContextTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_SetProperties()
    {
        var provider = new FakeDataProvider("TestProvider");
        var services = new ServiceCollection().BuildServiceProvider();

        using var context = new DatabaseContext(
            "TestDb",
            "Server=test;Database=Test",
            "EntityFramework",
            provider,
            services);

        Assert.Equal("TestDb", context.Name);
        Assert.NotNull(context.Connection);
        Assert.Equal("TestDb", context.Connection.Name);
        Assert.Equal("EntityFramework", context.Connection.ProviderName);
        Assert.Equal("Server=test;Database=Test", context.Connection.ConnectionString);
        Assert.Same(provider, context.Provider);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_Throw_When_NameIsNull()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new DatabaseContext(
            null!, "cs", "EF", provider, services));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_Throw_When_ConnectionStringIsNull()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new DatabaseContext(
            "Test", null!, "EF", provider, services));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_Throw_When_ProviderIsNull()
    {
        var services = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => new DatabaseContext(
            "Test", "cs", "EF", null!, services));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_Throw_When_ServiceProviderIsNull()
    {
        var provider = new FakeDataProvider("Test");

        Assert.Throws<ArgumentNullException>(() => new DatabaseContext(
            "Test", "cs", "EF", provider, null!));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_Should_Throw_When_NoRepositoryRegistered()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection().BuildServiceProvider();

        using var context = new DatabaseContext(
            "Test", "cs", "EF", provider, services);

        var ex = Assert.Throws<InvalidOperationException>(() => context.GetRepository<object>());
        Assert.Contains("No repository registered", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_Should_ReturnRepository_When_Registered()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection();
        services.AddTransient<IRepository<object>>(_ => new FakeRepository());
        var sp = services.BuildServiceProvider();

        using var context = new DatabaseContext(
            "Test", "cs", "EF", provider, sp);

        var repo = context.GetRepository<object>();

        Assert.NotNull(repo);
        Assert.IsType<FakeRepository>(repo);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dispose_Should_NotThrow_When_CalledMultipleTimes()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection().BuildServiceProvider();

        var context = new DatabaseContext(
            "Test", "cs", "EF", provider, services);

        context.Dispose();
        context.Dispose(); // Should not throw
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetRepository_Should_Throw_When_Disposed()
    {
        var provider = new FakeDataProvider("Test");
        var services = new ServiceCollection().BuildServiceProvider();

        var context = new DatabaseContext(
            "Test", "cs", "EF", provider, services);
        context.Dispose();

        Assert.Throws<ObjectDisposedException>(() => context.GetRepository<object>());
    }

    /// <summary>
    /// Fake repository for testing.
    /// </summary>
    private sealed class FakeRepository : IRepository<object>
    {
        public Task<object?> FindAsync(object id, CancellationToken cancellationToken = default)
            => Task.FromResult<object?>(null);

        public Task<PagedResult<object>> GetAllAsync(RepositoryQueryOptions? options = null, CancellationToken cancellationToken = default)
            => Task.FromResult(new PagedResult<object>(Array.Empty<object>(), 0, 1, 0));

        public Task InsertAsync(object entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(object entity, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task DeleteAsync(object id, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
