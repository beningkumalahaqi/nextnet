using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class AdapterRegistryTests
{
    private static (AdapterRegistry Registry, IServiceProvider Services) CreateRegistry()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new EdgeOptions());
        services.AddTransient<CloudflareWorkersAdapter>();
        services.AddTransient<VercelEdgeAdapter>();
        services.AddTransient<DenoDeployAdapter>();
        services.AddTransient<AwsLambdaEdgeAdapter>();
        var sp = services.BuildServiceProvider();
        var registry = new AdapterRegistry(sp);
        return (registry, sp);
    }

    [Fact]
    public void RegisterAndGetAdapter_ReturnsAdapter()
    {
        // Arrange
        var (registry, _) = CreateRegistry();
        registry.Register<CloudflareWorkersAdapter>();

        // Act
        var adapter = registry.GetAdapter("cloudflare");

        // Assert
        Assert.NotNull(adapter);
        Assert.Equal("Cloudflare Workers", adapter.ProviderName);
    }

    [Fact]
    public void Register_WithCustomProviderId_UsesCustomId()
    {
        // Arrange
        var (registry, _) = CreateRegistry();
        registry.Register<CloudflareWorkersAdapter>("my-cf");

        // Act
        var adapter = registry.GetAdapter("my-cf");

        // Assert
        Assert.NotNull(adapter);
    }

    [Fact]
    public void Register_DuplicateProviderId_Throws()
    {
        // Arrange
        var (registry, _) = CreateRegistry();
        registry.Register<CloudflareWorkersAdapter>("test");

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            registry.Register<CloudflareWorkersAdapter>("test"));
    }

    [Fact]
    public void GetAdapter_UnknownProvider_Throws()
    {
        // Arrange
        var (registry, _) = CreateRegistry();

        // Act & Assert
        Assert.Throws<KeyNotFoundException>(() =>
            registry.GetAdapter("nonexistent"));
    }

    [Fact]
    public void TryGetAdapter_KnownProvider_ReturnsTrue()
    {
        // Arrange
        var (registry, _) = CreateRegistry();
        registry.Register<CloudflareWorkersAdapter>();

        // Act
        var found = registry.TryGetAdapter("cloudflare", out var adapter);

        // Assert
        Assert.True(found);
        Assert.NotNull(adapter);
    }

    [Fact]
    public void TryGetAdapter_UnknownProvider_ReturnsFalse()
    {
        // Arrange
        var (registry, _) = CreateRegistry();

        // Act
        var found = registry.TryGetAdapter("nonexistent", out var adapter);

        // Assert
        Assert.False(found);
        Assert.Null(adapter);
    }

    [Fact]
    public void GetRegisteredProviders_ReturnsAll()
    {
        // Arrange
        var (registry, _) = CreateRegistry();
        registry.Register<CloudflareWorkersAdapter>();
        registry.Register<VercelEdgeAdapter>();

        // Act
        var providers = registry.GetRegisteredProviders().ToList();

        // Assert
        Assert.Contains("cloudflare", providers);
        Assert.Contains("vercel", providers);
    }

    [Fact]
    public void Register_Factory_ResolvesAdapter()
    {
        // Arrange
        var (registry, sp) = CreateRegistry();
        registry.Register("custom", () => new CloudflareWorkersAdapter());

        // Act
        var adapter = registry.GetAdapter("custom");

        // Assert
        Assert.NotNull(adapter);
        Assert.Equal("Cloudflare Workers", adapter.ProviderName);
    }

    [Fact]
    public void Register_FactoryNullProviderId_Throws()
    {
        var (registry, _) = CreateRegistry();
        Assert.Throws<ArgumentNullException>(() =>
            registry.Register(null!, () => new CloudflareWorkersAdapter()));
    }

    [Fact]
    public void Register_FactoryNullFactory_Throws()
    {
        var (registry, _) = CreateRegistry();
        Assert.Throws<ArgumentNullException>(() =>
            registry.Register("test", (Func<IEdgeRuntimeAdapter>)null!));
    }
}
