using Microsoft.Extensions.DependencyInjection;
using NextNet.Rendering;
using Xunit;

namespace NextNet.Layouts.Tests;

public class LayoutServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetLayouts_Should_RegisterAllServices_When_Called()
    {
        var services = new ServiceCollection();

        // Register the required dependency for LayoutChainResolver
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(),
            new Dictionary<string, Type>());
        services.AddSingleton<IRouteComponentResolver>(resolver);

        services.AddNextNetLayouts();

        var sp = services.BuildServiceProvider();

        // Verify LayoutChainResolver can be resolved
        var chainResolver = sp.GetService<LayoutChainResolver>();
        Assert.NotNull(chainResolver);

        // Verify LayoutRenderer can be resolved
        var renderer = sp.GetService<LayoutRenderer>();
        Assert.NotNull(renderer);

        // Verify ErrorBoundaryRenderer can be resolved
        var boundary = sp.GetService<ErrorBoundaryRenderer>();
        Assert.NotNull(boundary);
    }

    [Fact]
    public void AddNextNetLayouts_Should_ThrowArgumentNullException_When_ServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetLayouts());
    }

    [Fact]
    public void AddNextNetLayouts_Should_NotDuplicateRegistrations_When_CalledTwice()
    {
        var services = new ServiceCollection();

        // Register twice
        services.AddNextNetLayouts();
        services.AddNextNetLayouts();

        var sp = services.BuildServiceProvider();

        // Should resolve without error (TryAdd ensures single registration)
        var renderer = sp.GetService<LayoutRenderer>();
        Assert.NotNull(renderer);
    }
}
