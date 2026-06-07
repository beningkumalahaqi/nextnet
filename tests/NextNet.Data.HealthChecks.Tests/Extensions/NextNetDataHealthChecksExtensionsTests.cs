using Microsoft.Extensions.Diagnostics.HealthChecks;
using NextNet.Data.HealthChecks.Internal;

namespace NextNet.Data.HealthChecks.Tests.Extensions;

public class NextNetDataHealthChecksExtensionsTests
{
    [Fact]
    public void AddNextNetHealthChecks_Should_RegisterHealthCheckOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetHealthChecks();
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetService<IOptions<NextNetDataHealthCheckOptions>>();
        Assert.NotNull(options);
        Assert.Equal("/health", options.Value.EndpointPath);
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_RegisterNextNetDataHealthCheck()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddNextNetHealthChecks();
        var provider = services.BuildServiceProvider();

        // Assert
        var healthCheck = provider.GetService<IHealthCheck>();
        Assert.NotNull(healthCheck);
        Assert.IsType<NextNetDataHealthCheck>(healthCheck);
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_RegisterHealthCheckResultCache()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetHealthChecks();
        var provider = services.BuildServiceProvider();

        // Assert
        var cache = provider.GetService<HealthCheckResultCache>();
        Assert.NotNull(cache);
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_ApplyConfigurationAction()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetHealthChecks(options =>
        {
            options.EndpointPath = "/health/custom";
            options.ShowDetails = true;
            options.CacheTtl = TimeSpan.FromSeconds(30);
        });
        var provider = services.BuildServiceProvider();

        // Assert
        var options = provider.GetRequiredService<IOptions<NextNetDataHealthCheckOptions>>();
        Assert.Equal("/health/custom", options.Value.EndpointPath);
        Assert.True(options.Value.ShowDetails);
        Assert.Equal(TimeSpan.FromSeconds(30), options.Value.CacheTtl);
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_Throw_When_ServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetHealthChecks());
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_BeIdempotent()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act - calling multiple times should not throw
        services.AddNextNetHealthChecks();
        services.AddNextNetHealthChecks();
        services.AddNextNetHealthChecks();

        var provider = services.BuildServiceProvider();

        // Assert - still resolves
        var healthCheck = provider.GetService<IHealthCheck>();
        Assert.NotNull(healthCheck);
        Assert.IsType<NextNetDataHealthCheck>(healthCheck);
    }

    [Fact]
    public void AddNextNetHealthChecks_Should_RegisterAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddNextNetHealthChecks();
        var provider = services.BuildServiceProvider();

        // Assert - same instance
        var healthCheck1 = provider.GetService<IHealthCheck>();
        var healthCheck2 = provider.GetService<IHealthCheck>();
        Assert.Same(healthCheck1, healthCheck2);

        var options1 = provider.GetService<IOptions<NextNetDataHealthCheckOptions>>();
        var options2 = provider.GetService<IOptions<NextNetDataHealthCheckOptions>>();
        Assert.Same(options1, options2);

        var cache1 = provider.GetService<HealthCheckResultCache>();
        var cache2 = provider.GetService<HealthCheckResultCache>();
        Assert.Same(cache1, cache2);
    }
}
