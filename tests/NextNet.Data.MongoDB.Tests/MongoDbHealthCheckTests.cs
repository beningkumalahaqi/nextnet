namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoDbHealthCheck"/>.
/// </summary>
public sealed class MongoDbHealthCheckTests
{
    [Fact]
    public void Constructor_ShouldThrow_WhenClientManagerNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MongoDbHealthCheck(null!, new[] { "Default" }));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenConnectionNamesNull()
    {
        var manager = CreateEmptyManager();
        Assert.Throws<ArgumentNullException>(() =>
            new MongoDbHealthCheck(manager, null!));
    }

    [Fact]
    public void Name_ShouldReturnMongoDB()
    {
        var check = CreateHealthCheck();
        Assert.Equal("MongoDB", check.Name);
    }

    [Fact]
    public async Task GetHealthCheckAsync_ShouldReturnUnhealthy_WhenNoConnections()
    {
        var check = CreateHealthCheck();
        var result = await check.GetHealthCheckAsync();
        Assert.False(result.IsHealthy);
        Assert.Equal("Unhealthy", result.Status);
    }

    private static MongoDbHealthCheck CreateHealthCheck()
    {
        var manager = CreateEmptyManager();
        return new MongoDbHealthCheck(manager, Array.Empty<string>(),
            new TestLogger<MongoDbHealthCheck>());
    }

    private static MongoClientManager CreateEmptyManager()
    {
        return new MongoClientManager(
            new Dictionary<string, ConnectionConfig>(),
            new MongoDbOptions(),
            logger: new TestLogger<MongoClientManager>());
    }
}
