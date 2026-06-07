namespace NextNet.Data.EntityFramework.Tests;

/// <summary>
/// Tests for <see cref="EfCoreHealthCheckProvider"/>.
/// </summary>
public sealed class EfCoreHealthCheckProviderTests : IDisposable
{
    private readonly InMemoryDbContextFactory _fixture = new();

    [Fact]
    public async Task GetHealthCheckAsync_Should_ReturnHealthy_When_DatabaseIsReachable()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var provider = new EfCoreHealthCheckProvider(
            factory,
            new[] { "Default" },
            new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreHealthCheckProvider>());

        // Act
        var result = await provider.GetHealthCheckAsync();

        // Assert
        Assert.True(result.IsHealthy);
        Assert.Equal("Healthy", result.Status);
        Assert.NotNull(result.Message);
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_IncludeDuration()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var provider = new EfCoreHealthCheckProvider(
            factory,
            new[] { "Default" });

        // Act
        var result = await provider.GetHealthCheckAsync();

        // Assert
        Assert.True(result.Duration.TotalMilliseconds >= 0);
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_IncludePerConnectionData()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var provider = new EfCoreHealthCheckProvider(
            factory,
            new[] { "Primary", "Secondary" });

        // Act
        var result = await provider.GetHealthCheckAsync();

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("Primary"));
        Assert.True(result.Data.ContainsKey("Secondary"));
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_ReturnUnhealthy_When_ContextFactoryFails()
    {
        // Arrange - Use a factory that throws
        var failingFactory = new FailingDbContextFactory();
        var provider = new EfCoreHealthCheckProvider(
            failingFactory,
            new[] { "Default" });

        // Act
        var result = await provider.GetHealthCheckAsync();

        // Assert
        Assert.False(result.IsHealthy);
        Assert.Equal("Unhealthy", result.Status);
    }

    [Fact]
    public async Task GetHealthCheckAsync_Should_Handle_Cancellation()
    {
        // Arrange
        var factory = _fixture.CreateFactory();
        var provider = new EfCoreHealthCheckProvider(
            factory,
            new[] { "Default" });

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act - cancellation may or may not throw depending on timing
        try
        {
            var result = await provider.GetHealthCheckAsync(cts.Token);
            Assert.NotNull(result);
        }
        catch (OperationCanceledException)
        {
            // Expected in some cases
        }
    }

    [Fact]
    public void Constructor_Should_Throw_When_FactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EfCoreHealthCheckProvider(null!, new[] { "Default" }));
    }

    [Fact]
    public void Constructor_Should_Throw_When_ConnectionNamesIsNull()
    {
        using var factory = new InMemoryDbContextFactory();
        Assert.Throws<ArgumentNullException>(() =>
            new EfCoreHealthCheckProvider(factory.CreateFactory(), null!));
    }

    public void Dispose()
    {
        _fixture.Dispose();
    }
}

/// <summary>
/// A factory that always fails to create a DbContext, used for testing unhealthy scenarios.
/// </summary>
internal sealed class FailingDbContextFactory : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext()
    {
        throw new InvalidOperationException("Simulated factory failure.");
    }

    public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException("Simulated factory failure.");
    }
}
