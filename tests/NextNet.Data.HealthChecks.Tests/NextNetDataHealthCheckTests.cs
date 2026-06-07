using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NextNet.Data.HealthChecks.Tests;

public class NextNetDataHealthCheckTests
{
    private static NextNetDataHealthCheck CreateHealthCheck(
        IEnumerable<IHealthCheckProvider>? providers = null,
        HealthCheckResultCache? cache = null,
        NextNetDataHealthCheckOptions? options = null)
    {
        var providerList = providers ?? Array.Empty<IHealthCheckProvider>();
        var logger = Substitute.For<ILogger<NextNetDataHealthCheck>>();
        var cacheInstance = cache ?? new HealthCheckResultCache();
        var opts = Options.Create(options ?? new NextNetDataHealthCheckOptions());
        return new NextNetDataHealthCheck(providerList, logger, cacheInstance, opts);
    }

    private static HealthCheckContext CreateContext() =>
        new() { Registration = new HealthCheckRegistration("test", Substitute.For<IHealthCheck>(), HealthStatus.Unhealthy, null) };

    private static IHealthCheckProvider CreateMockProvider(
        bool isHealthy,
        string status = "Healthy",
        string? message = null,
        TimeSpan? duration = null,
        IReadOnlyDictionary<string, object>? data = null)
    {
        var provider = Substitute.For<IHealthCheckProvider>();
        var result = new Models.HealthCheckResult(
            isHealthy,
            status,
            duration ?? TimeSpan.FromMilliseconds(10),
            message,
            data);
        provider.GetHealthCheckAsync(default).ReturnsForAnyArgs(result);
        return provider;
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ReturnHealthy_When_AllProvidersHealthy()
    {
        // Arrange
        var providers = new[]
        {
            CreateMockProvider(true, "Healthy"),
            CreateMockProvider(true, "Healthy"),
        };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("All data providers are healthy", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ReturnUnhealthy_When_AnyProviderUnhealthy()
    {
        // Arrange
        var providers = new[]
        {
            CreateMockProvider(true, "Healthy"),
            CreateMockProvider(false, "Unhealthy", "Connection failed"),
        };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("Unhealthy provider(s)", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ReturnDegraded_When_AnyProviderDegraded()
    {
        // Arrange
        var providers = new[]
        {
            CreateMockProvider(true, "Healthy"),
            CreateMockProvider(true, "Degraded", "High latency"),
        };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Degraded, result.Status);
        Assert.Contains("Degraded provider(s)", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ReturnUnhealthy_When_MixedDegradedAndUnhealthy()
    {
        // Arrange
        var providers = new[]
        {
            CreateMockProvider(true, "Healthy"),
            CreateMockProvider(true, "Degraded", "Slow"),
            CreateMockProvider(false, "Unhealthy", "Down"),
        };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert - Unhealthy takes precedence over Degraded
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_NotThrow_When_ProviderThrows()
    {
        // Arrange
        var throwingProvider = Substitute.For<IHealthCheckProvider>();
        throwingProvider.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(Task.FromException<Models.HealthCheckResult>(new InvalidOperationException("Simulated failure")));

        var healthyProvider = CreateMockProvider(true, "Healthy");

        var providers = new[] { throwingProvider, healthyProvider };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert - exception is captured as Unhealthy, not thrown
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ExecuteAllChecks_When_MultipleProviders()
    {
        // Arrange
        var provider1 = Substitute.For<IHealthCheckProvider>();
        var provider2 = Substitute.For<IHealthCheckProvider>();

        provider1.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(new Models.HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(5)));
        provider2.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(new Models.HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(5)));

        var providers = new[] { provider1, provider2 };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act
        await healthCheck.CheckHealthAsync(context);

        // Assert - both providers were called
        await provider1.Received(1).GetHealthCheckAsync(Arg.Any<CancellationToken>());
        await provider2.Received(1).GetHealthCheckAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ReturnHealthy_When_NoProvidersRegistered()
    {
        // Arrange
        var healthCheck = CreateHealthCheck(Array.Empty<IHealthCheckProvider>());
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.Equal(HealthStatus.Healthy, result.Status);
        Assert.Contains("No data providers registered", result.Description);
    }

    [Fact]
    public async Task CheckHealthAsync_Should_CacheResult_When_CacheEnabled()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var provider = Substitute.For<IHealthCheckProvider>();
        provider.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(new Models.HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(5)));

        var providers = new[] { provider };
        var healthCheck = CreateHealthCheck(providers, cache);
        var context = CreateContext();

        // Act - first call should execute the check
        await healthCheck.CheckHealthAsync(context);
        var callCount = provider.ReceivedCalls().Count();

        // Second call should return cached result
        await healthCheck.CheckHealthAsync(context);

        // Assert - provider was not called again (cache hit)
        Assert.Equal(callCount, provider.ReceivedCalls().Count());
    }

    [Fact]
    public async Task CheckHealthAsync_Should_ExecuteFresh_When_CacheExpired()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var provider = Substitute.For<IHealthCheckProvider>();
        provider.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(new Models.HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(5)));

        var options = new NextNetDataHealthCheckOptions { CacheTtl = TimeSpan.FromMilliseconds(1) };
        var providers = new[] { provider };
        var healthCheck = CreateHealthCheck(providers, cache, options);
        var context = CreateContext();

        // Act - first call
        await healthCheck.CheckHealthAsync(context);

        // Wait for cache to expire
        await Task.Delay(10);

        // Second call - cache should be expired
        await healthCheck.CheckHealthAsync(context);

        // Assert - provider was called twice (first call + expired cache)
        await provider.Received(2).GetHealthCheckAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_Should_NotCache_When_CacheTtlIsZero()
    {
        // Arrange
        var cache = new HealthCheckResultCache();
        var provider = Substitute.For<IHealthCheckProvider>();
        provider.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs(new Models.HealthCheckResult(true, "Healthy", TimeSpan.FromMilliseconds(5)));

        var options = new NextNetDataHealthCheckOptions { CacheTtl = TimeSpan.Zero };
        var providers = new[] { provider };
        var healthCheck = CreateHealthCheck(providers, cache, options);
        var context = CreateContext();

        // Act - first call
        await healthCheck.CheckHealthAsync(context);

        // Second call - cache disabled, should execute again
        await healthCheck.CheckHealthAsync(context);

        // Assert - provider was called every time
        await provider.Received(2).GetHealthCheckAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CheckHealthAsync_Should_RespectCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var provider = Substitute.For<IHealthCheckProvider>();
        provider.GetHealthCheckAsync(default)
            .ReturnsForAnyArgs<Task<Models.HealthCheckResult>>(_ =>
                throw new TaskCanceledException("The operation was cancelled."));

        var providers = new[] { provider };
        var healthCheck = CreateHealthCheck(providers);
        var context = CreateContext();

        // Act & Assert
        cts.Cancel();
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            healthCheck.CheckHealthAsync(context, cts.Token));
    }

    [Fact]
    public async Task CheckHealthAsync_Should_IncludeData_WithProviderDetails()
    {
        // Arrange
        var providerData = new Dictionary<string, object>
        {
            { "database", "TestDb" },
            { "version", "1.0" }
        };

        var provider = CreateMockProvider(true, "Healthy", "OK", TimeSpan.FromMilliseconds(15), providerData);
        var healthCheck = CreateHealthCheck(new[] { provider });
        var context = CreateContext();

        // Act
        var result = await healthCheck.CheckHealthAsync(context);

        // Assert
        Assert.NotNull(result.Data);
        Assert.True(result.Data.ContainsKey("totalDurationMs"));
        Assert.True(result.Data.ContainsKey("totalProviders"));
        Assert.Equal(1, result.Data["totalProviders"]);
        Assert.Equal(1, result.Data["healthyCount"]);
    }

    [Fact]
    public void CheckHealthAsync_Should_Throw_When_ConstructorReceivesNull()
    {
        // Arrange
        var logger = Substitute.For<ILogger<NextNetDataHealthCheck>>();
        var cache = new HealthCheckResultCache();
        var options = Options.Create(new NextNetDataHealthCheckOptions());

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new NextNetDataHealthCheck(null!, logger, cache, options));
        Assert.Throws<ArgumentNullException>(() => new NextNetDataHealthCheck(
            Array.Empty<IHealthCheckProvider>(), null!, cache, options));
        Assert.Throws<ArgumentNullException>(() => new NextNetDataHealthCheck(
            Array.Empty<IHealthCheckProvider>(), logger, null!, options));
        Assert.Throws<ArgumentNullException>(() => new NextNetDataHealthCheck(
            Array.Empty<IHealthCheckProvider>(), logger, cache, null!));
    }
}
