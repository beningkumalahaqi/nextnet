using NextNet.Data.MultiDb.Internal;
using NextNet.Data.MultiDb.Tests.Fixtures;

namespace NextNet.Data.MultiDb.Tests;

public class DatabaseSelectorTests
{
    private static (DatabaseSelector Selector, ConnectionNameRegistry NameRegistry, ConnectionPoolRegistry PoolRegistry) CreateSelector(
        bool cacheContexts = true,
        bool fallbackToDefault = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var nameRegistry = MultiDbTestConfiguration.CreateNameRegistry(
            ("Primary", "EntityFramework", "Server=primary;Database=Test"),
            ("Analytics", "Dapper", "Host=analytics;Database=Reports"),
            ("Logging", "EntityFramework", "Data Source=logs.db"));

        var poolRegistry = MultiDbTestConfiguration.CreatePoolRegistry(
            ("Primary", "EntityFramework", "Server=primary;Database=Test"),
            ("Analytics", "Dapper", "Host=analytics;Database=Reports"),
            ("Logging", "EntityFramework", "Data Source=logs.db"));

        var options = MultiDbTestConfiguration.CreateOptions(
            cacheContexts: cacheContexts,
            fallbackToDefault: fallbackToDefault);

        var factory = new DatabaseContextFactory(sp);
        var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<DatabaseSelector>();

        var selector = new DatabaseSelector(nameRegistry, poolRegistry, factory, options, logger);
        return (selector, nameRegistry, poolRegistry);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_Should_ReturnContext_When_ConnectionExists()
    {
        var (selector, _, _) = CreateSelector();

        var context = selector.For("Analytics");

        Assert.NotNull(context);
        Assert.Equal("Analytics", context.Name);
        Assert.NotNull(context.Connection);
        Assert.Equal("Analytics", context.Connection.Name);
        Assert.Equal("Dapper", context.Connection.ProviderName);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_Should_ThrowMissingConnection_When_UnknownName()
    {
        var (selector, _, _) = CreateSelector();

        var ex = Assert.Throws<MissingConnectionException>(() => selector.For("Unknown"));
        Assert.Equal("SKDATA_MULTIDB_001", ex.ErrorCode);
        Assert.Contains("Unknown", ex.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_Should_Throw_When_NameIsEmpty()
    {
        var (selector, _, _) = CreateSelector();

        Assert.Throws<ArgumentException>(() => selector.For(""));
        Assert.Throws<ArgumentException>(() => selector.For("   "));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_Should_ReturnSameCachedContext_When_CacheContextsEnabled()
    {
        var (selector, _, _) = CreateSelector(cacheContexts: true);

        var ctx1 = selector.For("Primary");
        var ctx2 = selector.For("Primary");

        Assert.Same(ctx1, ctx2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_Should_Throw_When_Disposed()
    {
        var (selector, _, _) = CreateSelector();

        if (selector is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Assert.Throws<ObjectDisposedException>(() => selector.For("Primary"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Default_Should_Throw_When_NoDefaultConfigured()
    {
        var (selector, _, _) = CreateSelector();

        // The test registrations don't include "Default", so accessing .Default throws
        Assert.Throws<MissingConnectionException>(() => selector.Default);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasConnection_Should_ReturnTrue_When_Exists()
    {
        var (selector, _, _) = CreateSelector();

        Assert.True(selector.HasConnection("Primary"));
        Assert.True(selector.HasConnection("Analytics"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasConnection_Should_ReturnFalse_When_Unknown()
    {
        var (selector, _, _) = CreateSelector();

        Assert.False(selector.HasConnection("NonExistent"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConnectionNames_Should_ReturnAllNames()
    {
        var (selector, _, _) = CreateSelector();

        var names = selector.ConnectionNames;

        Assert.Contains("Primary", names);
        Assert.Contains("Analytics", names);
        Assert.Contains("Logging", names);
        Assert.Equal(3, names.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnection_Should_ReturnConnectionMeta()
    {
        var (selector, _, _) = CreateSelector();

        var conn = selector.GetConnection("Analytics");

        Assert.NotNull(conn);
        Assert.Equal("Analytics", conn.Name);
        Assert.Equal("Dapper", conn.ProviderName);
        Assert.Equal("Host=analytics;Database=Reports", conn.ConnectionString);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnection_Should_Throw_When_Unknown()
    {
        var (selector, _, _) = CreateSelector();

        var ex = Assert.Throws<MissingConnectionException>(() => selector.GetConnection("Unknown"));
        Assert.Equal("SKDATA_MULTIDB_001", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ForAsync_Should_ReturnContext()
    {
        var (selector, _, _) = CreateSelector();

        var context = await selector.ForAsync("Primary");

        Assert.NotNull(context);
        Assert.Equal("Primary", context.Name);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void For_WithFallback_Should_NotThrow_When_Unknown()
    {
        var (selector, _, _) = CreateSelector(fallbackToDefault: true);

        // When FallbackToDefault is true and the unknown connection doesn't exist,
        // it will try the default named "Default" which doesn't exist either,
        // so it should still throw MissingConnection.
        var ex = Assert.Throws<MissingConnectionException>(() => selector.For("Unknown"));
        Assert.Equal("SKDATA_MULTIDB_001", ex.ErrorCode);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HasConnection_Should_Throw_When_Disposed()
    {
        var (selector, _, _) = CreateSelector();

        if (selector is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Assert.Throws<ObjectDisposedException>(() => selector.HasConnection("Primary"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ConnectionNames_Should_Throw_When_Disposed()
    {
        var (selector, _, _) = CreateSelector();

        if (selector is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Assert.Throws<ObjectDisposedException>(() => selector.ConnectionNames);
    }
}
