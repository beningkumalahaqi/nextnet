using Microsoft.Extensions.DependencyInjection;
using NextNet.Isr.Background;
using NextNet.Isr.Cache;
using NextNet.Isr.Endpoints;
using NextNet.Isr.Manifest;
using NextNet.Isr.Revalidation;

namespace NextNet.Isr.Tests;

public class IsrServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetIsr_RegistersAllServices()
    {
        var services = new ServiceCollection();

        // We need to add the SSR dependencies that ISR depends on
        services.AddSingleton(new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>()));

        // Add NextNet rendering (we'll use minimal stubs)
        services.AddScoped<Rendering.SsrRenderer>(sp =>
        {
            var manifest = sp.GetRequiredService<Routing.RouteManifest>();
            return new Rendering.SsrRenderer(
                sp,
                manifest,
                new Rendering.SsrOptions());
        });

        services.AddNextNetIsr(options =>
        {
            options.DefaultRevalidateSeconds = 120;
            options.RevalidationSecret = "test-secret";
        });

        var provider = services.BuildServiceProvider();

        // Verify all key services are registered
        Assert.NotNull(provider.GetService<IIsrCacheStore>());
        Assert.NotNull(provider.GetService<IIsrRevalidationManager>());
        Assert.NotNull(provider.GetService<TimeBasedRevalidator>());
        Assert.NotNull(provider.GetService<OnDemandRevalidator>());
        Assert.NotNull(provider.GetService<WebhookRevalidator>());
        Assert.NotNull(provider.GetService<IsrManifestGenerator>());
        Assert.NotNull(provider.GetService<Manifest.IsrManifest>());
        Assert.NotNull(provider.GetService<RevalidationQueue>());
        Assert.NotNull(provider.GetService<IsrRevalidationEndpoint>());

        // Verify global options
        var globalOptions = provider.GetRequiredService<IsrGlobalOptions>();
        Assert.Equal(120, globalOptions.DefaultRevalidateSeconds);
        Assert.Equal("test-secret", globalOptions.RevalidationSecret);
    }

    [Fact]
    public void AddNextNetIsr_WithoutConfigure_UsesDefaults()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>()));

        services.AddScoped<Rendering.SsrRenderer>(sp =>
        {
            var manifest = sp.GetRequiredService<Routing.RouteManifest>();
            return new Rendering.SsrRenderer(sp, manifest);
        });

        services.AddNextNetIsr();

        var provider = services.BuildServiceProvider();
        var globalOptions = provider.GetRequiredService<IsrGlobalOptions>();

        Assert.Equal(60, globalOptions.DefaultRevalidateSeconds);
        Assert.Equal(4, globalOptions.MaxConcurrentRegenerations);
    }

    [Fact]
    public void AddNextNetIsr_NullServices_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetIsr());
    }

    [Fact]
    public void AddNextNetIsr_WithInvalidOptions_Throws()
    {
        var services = new ServiceCollection();

        services.AddSingleton(new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>()));

        services.AddScoped<Rendering.SsrRenderer>(sp =>
        {
            var manifest = sp.GetRequiredService<Routing.RouteManifest>();
            return new Rendering.SsrRenderer(sp, manifest);
        });

        Assert.Throws<InvalidOperationException>(() =>
            services.AddNextNetIsr(options =>
            {
                options.MaxConcurrentRegenerations = 0;
            }));
    }
}
