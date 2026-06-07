using Microsoft.AspNetCore.Http;
using Moq;
using NextNet.Isr.Background;
using NextNet.Isr.Cache;
using NextNet.Isr.Manifest;
using NextNet.Isr.Middleware;
using NextNet.Rendering;

namespace NextNet.Isr.Tests;

public class IsrMiddlewareTests
{
    private readonly Mock<IIsrCacheStore> _mockCacheStore;
    private readonly IsrManifest _isrManifest;
    private readonly RevalidationQueue _queue;
    private readonly SsrRenderer _ssrRenderer;

    public IsrMiddlewareTests()
    {
        _mockCacheStore = new Mock<IIsrCacheStore>(MockBehavior.Strict);
        _queue = new RevalidationQueue(capacity: 10);

        var routes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/cached"] = new() { RevalidateSeconds = 60 },
            ["/stale"] = new() { RevalidateSeconds = 60 }
        };

        _isrManifest = new IsrManifest(routes, new IsrGlobalOptions());

        var routeManifest = new Routing.RouteManifest(
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            Array.Empty<Routing.RouteEntry>(),
            null,
            Array.Empty<Routing.Models.RouteConflict>());

        _ssrRenderer = new SsrRenderer(Mock.Of<IServiceProvider>(), routeManifest);
    }

    private IsrMiddleware CreateMiddleware(RequestDelegate next)
    {
        return new IsrMiddleware(
            next,
            _mockCacheStore.Object,
            _ssrRenderer,
            _isrManifest,
            _queue);
    }

    [Fact]
    public async Task InvokeAsync_NonGet_ForwardsToNext()
    {
        var forwarded = false;
        var middleware = CreateMiddleware(ctx =>
        {
            forwarded = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "POST";

        await middleware.InvokeAsync(context);

        Assert.True(forwarded);
    }

    [Fact]
    public async Task InvokeAsync_CacheHitFresh_ServesCached()
    {
        var now = DateTime.UtcNow;
        var entry = new CacheEntry("/cached", now, 60);
        var cached = new CachedPage("/cached", "<html>fresh</html>", entry);

        _mockCacheStore.Setup(c => c.GetAsync("/cached", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/cached";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_CacheHitStale_ServesStaleAndEnqueuesRevalidation()
    {
        var oldTime = DateTime.UtcNow.AddSeconds(-120);
        var entry = new CacheEntry("/stale", oldTime, 60);
        var cached = new CachedPage("/stale", "<html>stale</html>", entry);

        _mockCacheStore.Setup(c => c.GetAsync("/stale", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/stale";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);

        // Verify revalidation was enqueued
        Assert.Equal(1, _queue.PendingCount);
    }

    [Fact]
    public async Task InvokeAsync_CacheMissNonIsrRoute_ForwardsToNext()
    {
        _mockCacheStore.Setup(c => c.GetAsync("/unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedPage?)null);

        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/unknown";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_AcceptsAllHtml_ProcessesRequest()
    {
        var entry = new CacheEntry("/cached", DateTime.UtcNow, 60);
        var cached = new CachedPage("/cached", "<html>content</html>", entry);

        _mockCacheStore.Setup(c => c.GetAsync("/cached", It.IsAny<CancellationToken>()))
            .ReturnsAsync(cached);

        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/cached";
        context.Request.Headers.Accept = "*/*";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.False(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_NonHtmlAccept_Skips()
    {
        var nextCalled = false;
        var middleware = CreateMiddleware(ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/data";
        context.Request.Headers.Accept = "application/json";

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task InvokeAsync_WhenIsrRouteCacheMiss_AndRouteNotInSsrManifest_FallsThrough()
    {
        var isrRoutes = new Dictionary<string, IsrRouteMetadata>
        {
            ["/isr-page"] = new() { RevalidateSeconds = 60 }
        };
        var isrManifest = new IsrManifest(isrRoutes, new IsrGlobalOptions());

        _mockCacheStore.Setup(c => c.GetAsync("/isr-page", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedPage?)null);

        var nextCalled = false;
        var middleware = new IsrMiddleware(
            ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            _mockCacheStore.Object,
            _ssrRenderer,
            isrManifest,
            _queue);

        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/isr-page";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        // Route doesn't exist in the SSR manifest, so should fall through to next middleware
        Assert.True(nextCalled);
    }
}
