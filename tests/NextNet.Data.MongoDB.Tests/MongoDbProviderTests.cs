namespace NextNet.Data.MongoDB.Tests;

/// <summary>
/// Tests for <see cref="MongoDbProvider"/>.
/// </summary>
public sealed class MongoDbProviderTests
{
    [Fact]
    public void Name_ShouldReturnMongoDB()
    {
        var provider = CreateProvider();
        Assert.Equal("MongoDB", provider.Name);
    }

    [Fact]
    public void DisplayName_ShouldReturnMongoDB()
    {
        var provider = CreateProvider();
        Assert.Equal("MongoDB 7.0+", provider.DisplayName);
    }

    [Fact]
    public void Version_ShouldNotBeNull()
    {
        var provider = CreateProvider();
        Assert.NotNull(provider.Version);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenOptionsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MongoDbProvider(null!, new TestLogger<MongoDbProvider>()));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenLoggerNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MongoDbProvider(new MongoDbOptions(), null!));
    }

    [Fact]
    public async Task InitializeAsync_ShouldThrow_WhenConfigNull()
    {
        var provider = CreateProvider();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            ((NextNet.Data.Abstractions.Abstractions.IDataProvider)provider)
                .InitializeAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void ProviderMetadata_ShouldBeDiscoverable()
    {
        var attr = typeof(MongoDbProvider).GetCustomAttributes(typeof(ProviderMetadataAttribute), false)
            .FirstOrDefault() as ProviderMetadataAttribute;
        Assert.NotNull(attr);
        Assert.Equal("MongoDB", attr!.Id);
        Assert.Equal("MongoDB 7.0+", attr.DisplayName);
        Assert.True(attr.SupportsRepositories);
        Assert.False(attr.SupportsMigrations);
    }

    private static MongoDbProvider CreateProvider()
    {
        return new MongoDbProvider(
            new MongoDbOptions(),
            new TestLogger<MongoDbProvider>());
    }
}

/// <summary>
/// Simple test logger that implements ILogger{T} for testing purposes.
/// </summary>
internal sealed class TestLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => false;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
