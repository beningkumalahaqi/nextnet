using Microsoft.AspNetCore.Http;
using Moq;
using NextNet.Components;
using NextNet.Isr.Cache;
using NextNet.Isr.Revalidation;
using NextNet.Rendering;

namespace NextNet.Isr.Tests;

/// <summary>
/// Additional tests for IsrRevalidationManager that exercise the RevalidateAsync
/// path which requires SSR rendering.
/// </summary>
public class IsrRevalidationManagerFullTests
{
    [Fact]
    public async Task RevalidateAsync_WithNonExistentRoute_ReturnsFailure()
    {
        var cacheStore = new Mock<IIsrCacheStore>(MockBehavior.Strict);
        var routeManifest = new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>());

        var serviceProvider = Mock.Of<IServiceProvider>();
        var ssrRenderer = new SsrRenderer(serviceProvider, routeManifest);

        var httpContextAccessor = new Mock<IHttpContextAccessor>(MockBehavior.Strict);
        httpContextAccessor.Setup(a => a.HttpContext).Returns(new DefaultHttpContext());

        var manager = new IsrRevalidationManager(
            cacheStore.Object,
            ssrRenderer,
            httpContextAccessor.Object,
            new IsrGlobalOptions { DefaultRevalidateSeconds = 60 });

        // A route that doesn't exist in the manifest
        var result = await manager.RevalidateAsync("/nonexistent-route");

        Assert.False(result.Success);
    }
}
