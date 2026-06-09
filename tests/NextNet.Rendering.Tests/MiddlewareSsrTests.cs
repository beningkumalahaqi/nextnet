using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Rendering.Middleware;
using NextNet.Rendering.Streaming;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Rendering.Tests;

/// <summary>
/// Focused tests for the SsrMiddleware behavior beyond what integration tests cover.
/// </summary>
public class MiddlewareSsrTests
{
    // ─── Test page that throws ────────────────────────────────────────────

    private sealed class ThrowingPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render() =>
            throw new InvalidOperationException("Middleware render crash");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static (SsrMiddleware Middleware, SsrRenderer Ssr) CreateMiddleware(
        RouteManifest manifest,
        IReadOnlyDictionary<string, Type> pageMap,
        bool streaming = true)
    {
        var services = new ServiceCollection();
        foreach (var (_, type) in pageMap)
        {
            services.AddScoped(type);
        }
        var sp = services.BuildServiceProvider();

        var resolver = new ConventionRouteComponentResolver(pageMap,
            new Dictionary<string, Type>());
        var options = new SsrOptions
        {
            Streaming = streaming,
            BufferSize = 4096,
            RenderTimeout = TimeSpan.FromSeconds(5)
        };

        var ssr = new SsrRenderer(sp, manifest, options, resolver);
        var streamingRenderer = new StreamingHtmlRenderer(ssr, options);

        var middleware = new SsrMiddleware(
            _ => Task.CompletedTask,
            ssr,
            streamingRenderer,
            options);

        return (middleware, ssr);
    }

    private static DefaultHttpContext CreateContext(string path = "/",
        string method = "GET", string accept = "text/html")
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = method;
        ctx.Request.Path = path;
        ctx.Request.Headers.Accept = accept;
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_Should_WriteChunks_WhenStreamingAndValidRoute()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(SimplePage)
        };

        var (middleware, _) = CreateMiddleware(manifest, pageMap, streaming: true);
        var ctx = CreateContext("/", "GET", "text/html");

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", ctx.Response.ContentType);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("<p>Hello from SSR</p>", body);
    }

    [Fact]
    public async Task InvokeAsync_Should_Return500_WhenStandardRenderFails()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/fail", "app/fail/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/fail", "app/fail/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var pageMap = new Dictionary<string, Type>
        {
            ["app/fail/page.cs"] = typeof(ThrowingPage)
        };

        var (middleware, _) = CreateMiddleware(manifest, pageMap, streaming: false);
        var ctx = CreateContext("/fail", "GET", "text/html");

        await middleware.InvokeAsync(ctx);

        Assert.Equal(500, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("Internal Server Error", body);
    }

    private sealed class SimplePage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render() =>
            Task.FromResult(HtmlHelper.Element("p", content: HtmlHelper.Text("Hello from SSR")));
    }

    private sealed class SimpleErrorPage : IErrorPage
    {
        public Task<IHtmlContent> Render(Exception exception) =>
            Task.FromResult<IHtmlContent>(
                new RawHtmlContent($"<error>{exception.Message}</error>"));
    }

    // ─── Additional Edge Case Tests ──────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_WhenMethodIsPost()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var (middleware, _) = CreateMiddleware(manifest,
            new Dictionary<string, Type>(), streaming: true);
        var ctx = CreateContext("/", "POST", "text/html");

        // Should pass through to next middleware without writing response
        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode); // Status not set by middleware
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Empty(body);
    }

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_WhenAcceptIsNonHtml()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var (middleware, _) = CreateMiddleware(manifest,
            new Dictionary<string, Type>(), streaming: true);
        var ctx = CreateContext("/", "GET", "application/json");

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Empty(body);
    }

    [Fact]
    public async Task InvokeAsync_Should_PassThrough_WhenRouteIsUnknown()
    {
        var manifest = new RouteManifest(
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var (middleware, _) = CreateMiddleware(manifest,
            new Dictionary<string, Type>(), streaming: true);
        var ctx = CreateContext("/unknown", "GET", "text/html");

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Empty(body);
    }

    [Fact]
    public async Task InvokeAsync_Should_NotSetStatus_WhenNotFoundAndStandard()
    {
        var manifest = new RouteManifest(
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var (middleware, _) = CreateMiddleware(manifest,
            new Dictionary<string, Type>(), streaming: false);

        // Even with no routes, the middleware should pass through (not crash)
        var ctx = CreateContext("/nonexistent", "GET", "text/html");
        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_Should_UseStandardPath_WhenStreamingDisabled()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(SimplePage)
        };

        var (middleware, _) = CreateMiddleware(manifest, pageMap, streaming: false);
        var ctx = CreateContext("/", "GET", "text/html");

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("<p>Hello from SSR</p>", body);
    }

    [Fact]
    public async Task InvokeAsync_Should_HandleRequest_WhenAcceptHeaderIsEmpty()
    {
        var manifest = new RouteManifest(
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            new[] { new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static) },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(SimplePage)
        };

        var (middleware, _) = CreateMiddleware(manifest, pageMap, streaming: true);
        var ctx = CreateContext("/", "GET", ""); // Empty accept header

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("<p>Hello from SSR</p>", body);
    }
}
