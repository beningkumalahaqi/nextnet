using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Adapters;
using NextNet.Edge.Build;
using NextNet.Edge.Compatibility;
using Xunit;

namespace NextNet.Edge.Tests;

public class EdgeServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetEdge_Should_RegisterAllServices_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetEdge(new EdgeOptions());
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetService<EdgeOptions>());
        Assert.NotNull(sp.GetService<AdapterRegistry>());
        Assert.NotNull(sp.GetService<EdgeCompatibilityChecker>());
        Assert.NotNull(sp.GetService<EdgeApiWhitelist>());
        Assert.NotNull(sp.GetService<EdgeBuildStep>());
        Assert.NotNull(sp.GetService<EdgeEntryGenerator>());
        Assert.NotNull(sp.GetService<CloudflareWorkersAdapter>());
        Assert.NotNull(sp.GetService<VercelEdgeAdapter>());
        Assert.NotNull(sp.GetService<DenoDeployAdapter>());
        Assert.NotNull(sp.GetService<AwsLambdaEdgeAdapter>());
    }

    [Fact]
    public void AddNextNetEdge_Should_RegisterServices_When_UsingDefaults()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetEdge();
        var sp = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(sp.GetService<EdgeOptions>());
    }

    [Fact]
    public void AddNextNetEdge_Should_ApplyConfiguration_When_UsingConfigureDelegate()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNextNetEdge(options =>
        {
            options.Enabled = true;
            options.Provider = "vercel";
        });
        var sp = services.BuildServiceProvider();

        // Assert
        var opts = sp.GetRequiredService<EdgeOptions>();
        Assert.True(opts.Enabled);
        Assert.Equal("vercel", opts.Provider);
    }

    [Fact]
    public void AddNextNetEdge_Should_Throw_When_ServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetEdge());
    }

    [Fact]
    public void AddNextNetEdge_Should_Throw_When_ConfigureIsNull()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() =>
            services.AddNextNetEdge((Action<EdgeOptions>)null!));
    }
}
