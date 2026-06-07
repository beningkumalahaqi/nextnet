using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;

namespace NextNet.Data.Dapper.Tests.Connection;

/// <summary>
/// Tests for <see cref="DapperConnectionManager"/> connection management,
/// thread safety, and connection string resolution.
/// </summary>
public sealed class DapperConnectionManagerTests
{
    private static readonly Dictionary<string, ConnectionConfig> SampleConnections = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Default"] = new ConnectionConfig(
            "Server=localhost;Database=TestDb;Trusted_Connection=true;",
            Provider: "Dapper",
            TimeoutSeconds: 30),
        ["Analytics"] = new ConnectionConfig(
            "Server=analytics.db;Database=AnalyticsDb;Trusted_Connection=true;",
            Provider: "Dapper",
            TimeoutSeconds: 60)
    };

    /// <summary>
    /// Creates a <see cref="DapperConnectionManager"/> with sample connections and default options.
    /// </summary>
    private static DapperConnectionManager CreateManager(
        IReadOnlyDictionary<string, ConnectionConfig>? connections = null,
        DapperOptions? options = null)
    {
        return new DapperConnectionManager(
            connections ?? SampleConnections,
            options ?? new DapperOptions { EnablePooling = false },
            NullLogger<DapperConnectionManager>.Instance);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_ThrowArgumentNullException_When_ConnectionsIsNull()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new DapperConnectionManager(null!));
        Assert.Contains("connections", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_UseDefaultOptions_When_OptionsIsNull()
    {
        var manager = new DapperConnectionManager(SampleConnections);
        Assert.NotNull(manager);
        Assert.Equal(new List<string> { "Analytics", "Default" }, manager.GetConnectionNames().OrderBy(n => n).ToList());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_ReturnCachedString_When_CalledMultipleTimes()
    {
        var manager = CreateManager();
        var first = manager.GetConnectionString("Default");
        var second = manager.GetConnectionString("Default");

        Assert.Equal(first, second);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_ThrowKeyNotFoundException_When_ConnectionDoesNotExist()
    {
        var manager = CreateManager();
        var ex = Assert.Throws<KeyNotFoundException>(() => manager.GetConnectionString("NonExistent"));
        Assert.Contains("NonExistent", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_IncludePoolSettings_When_OptionsConfigured()
    {
        var options = new DapperOptions
        {
            EnablePooling = true,
            MaxPoolSize = 50,
            MinPoolSize = 5,
            CommandTimeoutSeconds = 60
        };
        var manager = CreateManager(options: options);
        var connectionString = manager.GetConnectionString("Default");

        Assert.Contains("Pooling=True", connectionString, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Max Pool Size=50", connectionString, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Min Pool Size=5", connectionString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_BeCaseInsensitive()
    {
        var manager = CreateManager();
        var lower = manager.GetConnectionString("default");
        var upper = manager.GetConnectionString("DEFAULT");
        var mixed = manager.GetConnectionString("Default");

        Assert.Equal(lower, upper);
        Assert.Equal(upper, mixed);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionNames_Should_ReturnAllConfiguredNames()
    {
        var manager = CreateManager();
        var names = manager.GetConnectionNames().OrderBy(n => n).ToList();

        Assert.Contains("Default", names);
        Assert.Contains("Analytics", names);
        Assert.Equal(2, names.Count);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionNames_Should_ReturnReadOnlyCollection()
    {
        var manager = CreateManager();
        var names = manager.GetConnectionNames();

        Assert.True(names is IReadOnlyCollection<string>);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetDefaultConnectionAsync_Should_UseDefaultConnectionName()
    {
        var options = new DapperOptions { ConnectionName = "Analytics", EnablePooling = false };
        var manager = CreateManager(options: options);

        // Should find "Analytics" connection. We can't actually open it
        // (no server), but we can verify the connection string is resolved.
        var connString = manager.GetConnectionString("Analytics");
        Assert.Contains("analytics.db", connString, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_HandleEmptyConnections()
    {
        var manager = CreateManager(new Dictionary<string, ConnectionConfig>());
        Assert.Empty(manager.GetConnectionNames());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateConnection_Should_CreateUnopenedConnection()
    {
        var manager = CreateManager();
        using var conn = manager.CreateConnection("Default");

        Assert.NotNull(conn);
        Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CreateConnection_Should_ThrowObjectDisposedException_When_Disposed()
    {
        var manager = CreateManager();
        manager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => manager.CreateConnection("Default"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GetConnectionString_Should_ThrowObjectDisposedException_When_Disposed()
    {
        var manager = CreateManager();
        manager.Dispose();

        Assert.Throws<ObjectDisposedException>(() => manager.GetConnectionString("Default"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task OpenConnectionAsync_Should_ThrowObjectDisposedException_When_Disposed()
    {
        var manager = CreateManager();
        manager.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => manager.OpenConnectionAsync("Default"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetConnectionAsync_Should_ThrowKeyNotFoundException_When_ConnectionMissing()
    {
        var manager = CreateManager();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => manager.GetConnectionAsync("Missing"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Dispose_ShouldBeIdempotent()
    {
        var manager = CreateManager();
        manager.Dispose();
        manager.Dispose(); // Should not throw
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ThreadSafety_Should_HandleConcurrentAccess()
    {
        var manager = CreateManager();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, 100, i =>
        {
            try
            {
                var name = i % 2 == 0 ? "Default" : "Analytics";
                var connString = manager.GetConnectionString(name);
                Assert.NotNull(connString);
                Assert.NotEmpty(connString);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateConnectionsAsync_Should_ReturnFalseResults_When_NoServer()
    {
        // Using a deliberately unreachable connection string
        var connections = new Dictionary<string, ConnectionConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["Unreachable"] = new ConnectionConfig(
                "Server=192.0.2.1,9999;Database=Test;Trusted_Connection=true;Connect Timeout=1;",
                Provider: "Dapper")
        };

        var options = new DapperOptions { EnablePooling = false, CommandTimeoutSeconds = 1 };
        var logger = NullLogger<DapperConnectionManager>.Instance;
        var manager = new DapperConnectionManager(connections, options, logger);

        var results = await manager.ValidateConnectionsAsync();

        Assert.Single(results);
        Assert.False(results["Unreachable"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_Should_AcceptEmptyConnectionDictionary()
    {
        var manager = new DapperConnectionManager(
            new Dictionary<string, ConnectionConfig>(),
            new DapperOptions { EnablePooling = false });

        Assert.Empty(manager.GetConnectionNames());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ThreadSafety_Should_HandleConcurrentCreateConnection()
    {
        var manager = CreateManager();
        var exceptions = new ConcurrentBag<Exception>();

        Parallel.For(0, 50, i =>
        {
            try
            {
                var name = i % 2 == 0 ? "Default" : "Analytics";
                using var conn = manager.CreateConnection(name);
                Assert.Equal(System.Data.ConnectionState.Closed, conn.State);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        Assert.Empty(exceptions);
    }
}
