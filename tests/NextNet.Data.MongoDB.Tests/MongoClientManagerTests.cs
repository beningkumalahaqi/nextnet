namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoClientManager"/>.
/// </summary>
public sealed class MongoClientManagerTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenConnectionsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MongoClientManager(null!));
    }

    [Fact]
    public void Constructor_ShouldAcceptEmptyConnections()
    {
        var manager = new MongoClientManager(
            new Dictionary<string, ConnectionConfig>(),
            new MongoDbOptions());
        Assert.NotNull(manager);
    }

    [Fact]
    public void GetConnectionString_ShouldThrow_WhenConnectionNotFound()
    {
        var manager = CreateManager();
        Assert.Throws<KeyNotFoundException>(() => manager.GetConnectionString("NonExistent"));
    }

    [Fact]
    public void GetConnectionString_ShouldReturn_WhenConnectionExists()
    {
        var manager = CreateManagerWithConnection();
        var connString = manager.GetConnectionString("Default");
        Assert.Equal("mongodb://localhost:27017", connString);
    }

    [Fact]
    public void GetConnectionNames_ShouldReturnRegisteredNames()
    {
        var manager = CreateManagerWithConnection();
        var names = manager.GetConnectionNames();
        Assert.Contains("Default", names);
    }

    [Fact]
    public void GetConnectionNames_ShouldReturnEmpty_WhenNoConnections()
    {
        var manager = CreateManager();
        Assert.Empty(manager.GetConnectionNames());
    }

    [Fact]
    public async Task GetDatabaseAsync_ShouldThrow_WhenDisposed()
    {
        var manager = CreateManagerWithConnection();
        manager.Dispose();
        await Assert.ThrowsAsync<ObjectDisposedException>(() =>
            manager.GetDatabaseAsync("Default", CancellationToken.None));
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenCalledMultipleTimes()
    {
        var manager = CreateManagerWithConnection();
        manager.Dispose();
        manager.Dispose(); // Should not throw
    }

    private static MongoClientManager CreateManager()
    {
        return new MongoClientManager(
            new Dictionary<string, ConnectionConfig>(),
            new MongoDbOptions(),
            logger: new TestLogger<MongoClientManager>());
    }

    private static MongoClientManager CreateManagerWithConnection()
    {
        var connections = new Dictionary<string, ConnectionConfig>(StringComparer.OrdinalIgnoreCase)
        {
            ["Default"] = new ConnectionConfig(
                ConnectionString: "mongodb://localhost:27017",
                Provider: "MongoDB")
        };

        return new MongoClientManager(
            connections,
            new MongoDbOptions { DefaultDatabaseName = "test" },
            logger: new TestLogger<MongoClientManager>());
    }
}
