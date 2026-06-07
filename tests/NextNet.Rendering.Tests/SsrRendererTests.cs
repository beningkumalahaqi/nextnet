using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Rendering.Tests;

public class SsrRendererTests
{
    // ─── Test doubles ─────────────────────────────────────────────────────

    private sealed class SimplePage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>
        {
            ["title"] = "Simple"
        };

        public Task<IHtmlContent> Render() =>
            Task.FromResult(HtmlHelper.Element("h1", content: HtmlHelper.Text("Simple Page")));
    }

    private sealed class ErrorPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public Task<IHtmlContent> Render() =>
            throw new InvalidOperationException("Simulated render failure");
    }

    private sealed class SlowPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

        public async Task<IHtmlContent> Render()
        {
            // Simulate a page that takes too long
            await Task.Delay(TimeSpan.FromSeconds(10));
            return HtmlHelper.Text("never");
        }
    }

    private sealed class TestErrorPage : IErrorPage
    {
        public Task<IHtmlContent> Render(Exception exception) =>
            Task.FromResult(HtmlHelper.Raw($"<div class=\"error\">Error: {exception.Message}</div>"));
    }

    // ─── Test data ────────────────────────────────────────────────────────

    private static RouteManifest CreateManifest(params RouteEntry[] pages)
    {
        return new RouteManifest(
            pages,
            pages.Where(p => p.Type == RouteType.Page).ToList(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());
    }

    private static RouteEntry PageEntry(string route, string filePath,
        RouteSegmentKind segmentKind = default,
        string[]? layoutChain = null)
    {
        var entry = new RouteEntry(route, filePath, RouteType.Page, segmentKind);
        entry.LayoutChain = (IReadOnlyList<string>)(layoutChain ?? Array.Empty<string>());
        return entry;
    }

    private static RouteEntry LayoutEntry(string route, string filePath)
    {
        return new RouteEntry(route, filePath, RouteType.Layout, RouteSegmentKind.Static);
    }

    private static RouteEntry ErrorEntry(string filePath = "app/error.cs")
    {
        return new RouteEntry("/_error", filePath, RouteType.Error, RouteSegmentKind.Static);
    }

    private static (SsrRenderer Renderer, IServiceProvider Services) CreateRenderer(
        RouteManifest manifest,
        Action<ServiceCollection>? configureServices = null,
        IReadOnlyDictionary<string, Type>? pageMap = null,
        IReadOnlyDictionary<string, Type>? layoutMap = null)
    {
        var services = new ServiceCollection();
        configureServices?.Invoke(services);
        var sp = services.BuildServiceProvider();

        var resolver = new ConventionRouteComponentResolver(
            pageMap ?? new Dictionary<string, Type>(),
            layoutMap ?? new Dictionary<string, Type>());

        var options = new SsrOptions
        {
            Streaming = false,
            RenderTimeout = TimeSpan.FromSeconds(5)
        };

        var renderer = new SsrRenderer(sp, manifest, options, resolver);
        return (renderer, sp);
    }

    private static ComponentContext CreateContext(HttpContext? httpContext = null)
    {
        // Use DefaultHttpContext if none provided
        var ctx = httpContext ?? new DefaultHttpContext();
        return new ComponentContext(ctx);
    }

    // ─── Route Resolution Tests ───────────────────────────────────────────

    [Fact]
    public void ResolveRoute_WithExactMatch_ReturnsEntry()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute("/");

        Assert.NotNull(result);
        Assert.Equal("/", result.RoutePattern);
    }

    [Fact]
    public void ResolveRoute_WithNormalizedPath_ReturnsEntry()
    {
        var manifest = CreateManifest(PageEntry("/about", "app/about/page.cs"));
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute("about"); // no leading slash

        Assert.NotNull(result);
        Assert.Equal("/about", result.RoutePattern);
    }

    [Fact]
    public void ResolveRoute_WithUnknownRoute_ReturnsNull()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute("/nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void ResolveRoute_WithNull_ReturnsNull()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute(null!);

        Assert.Null(result);
    }

    [Fact]
    public void ResolveRoute_WithEmptyString_ReturnsNull()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute("");

        Assert.Null(result);
    }

    [Fact]
    public void ResolveRoute_WithParameterisedMatch_ReturnsEntry()
    {
        var manifest = CreateManifest(
            PageEntry("/blog/{slug}", "app/blog/[slug]/page.cs", RouteSegmentKind.Dynamic));
        var (renderer, _) = CreateRenderer(manifest);

        var result = renderer.ResolveRoute("/blog/hello-world");

        Assert.NotNull(result);
        Assert.Equal("/blog/{slug}", result.RoutePattern);
    }

    // ─── RenderAsync Tests ────────────────────────────────────────────────

    [Fact]
    public async Task RenderAsync_WithValidRoute_ReturnsHtmlResponse()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/page.cs"] = typeof(SimplePage) };
        var (renderer, _) = CreateRenderer(manifest,
            s => s.AddScoped<SimplePage>(),
            pageMap: pageMap);

        var context = CreateContext();
        var response = await renderer.RenderAsync("/", context);

        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Contains("Simple Page", response.ToString());
        Assert.Contains("<h1>", response.ToString());
    }

    [Fact]
    public async Task RenderAsync_WithNotFoundRoute_Returns404()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var (renderer, _) = CreateRenderer(manifest);

        var response = await renderer.RenderAsync("/missing", CreateContext());

        Assert.NotNull(response);
        Assert.Equal(404, response.StatusCode);
        Assert.Contains("404", response.ToString());
    }

    [Fact]
    public async Task RenderAsync_WithRenderFailure_ReturnsErrorPage()
    {
        var manifest = CreateManifest(PageEntry("/error", "app/error/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/error/page.cs"] = typeof(ErrorPage) };
        var (renderer, _) = CreateRenderer(manifest,
            s => s.AddScoped<ErrorPage>(),
            pageMap: pageMap);

        var response = await renderer.RenderAsync("/error", CreateContext());

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
        Assert.Contains("Internal Server Error", response.ToString());
    }

    [Fact]
    public async Task RenderAsync_WithTimeout_ReturnsErrorPage()
    {
        var manifest = CreateManifest(PageEntry("/slow", "app/slow/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/slow/page.cs"] = typeof(SlowPage) };
        var (renderer, _) = CreateRenderer(manifest,
            s => s.AddScoped<SlowPage>(),
            pageMap: pageMap);

        var response = await renderer.RenderAsync("/slow", CreateContext());

        Assert.NotNull(response);
        Assert.Equal(500, response.StatusCode);
        Assert.Contains("timed out", response.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderAsync_WithLayoutChain_ComposesFullDocument()
    {
        // Create a test layout that wraps content
        var manifest = CreateManifest(PageEntry("/", "app/page.cs", layoutChain: ["app/layout.cs"]));
        var pageMap = new Dictionary<string, Type> { ["app/page.cs"] = typeof(SimplePage) };
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(TestWrapLayout) };
        var (renderer, _) = CreateRenderer(manifest,
            s =>
            {
                s.AddScoped<SimplePage>();
                s.AddScoped<TestWrapLayout>();
            },
            pageMap: pageMap,
            layoutMap: layoutMap);

        var response = await renderer.RenderAsync("/", CreateContext());

        Assert.Equal(200, response.StatusCode);
        var html = response.ToString();
        Assert.Contains("<h1>Simple Page</h1>", html);
        Assert.Contains("<!--test-layout-start-->", html);
        Assert.Contains("<!--test-layout-end-->", html);
    }

    [Fact]
    public async Task RenderAsync_WithCustomErrorPage_RendersUserErrorPage()
    {
        var manifest = CreateManifest(
            PageEntry("/", "app/page.cs"));
        // Manually set error page
        var errorEntry = ErrorEntry("app/error.cs");

        var manifestWithError = new RouteManifest(
            manifest.Routes,
            manifest.Pages,
            manifest.Layouts,
            manifest.ApiRoutes,
            errorEntry,
            manifest.Conflicts);

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(SimplePage),
            ["app/error.cs"] = typeof(TestErrorPage)
        };
        var (renderer, _) = CreateRenderer(manifestWithError,
            s =>
            {
                s.AddScoped<SimplePage>();
                s.AddScoped<TestErrorPage>();
            },
            pageMap: pageMap);

        var response = await renderer.RenderAsync("/", CreateContext());

        Assert.Equal(200, response.StatusCode);
    }

    [Fact]
    public async Task RenderErrorAsync_WithoutCustomErrorPage_RendersBuiltIn()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var (renderer, _) = CreateRenderer(manifest);

        var ex = new InvalidOperationException("test error");
        var response = await renderer.RenderErrorAsync(ex);

        Assert.Equal(500, response.StatusCode);
        Assert.Contains("Internal Server Error", response.ToString());
    }

    [Fact]
    public async Task RenderErrorAsync_WithErrorPageRendererThrows_FallsBackToBuiltIn()
    {
        // Create manifest with error page but without registering it in DI
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var errorEntry = ErrorEntry("app/error.cs");

        var manifestWithError = new RouteManifest(
            manifest.Routes,
            manifest.Pages,
            manifest.Layouts,
            manifest.ApiRoutes,
            errorEntry,
            manifest.Conflicts);

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(SimplePage),
            ["app/error.cs"] = typeof(TestErrorPage) // type exists but not registered in DI
        };
        var (renderer, _) = CreateRenderer(manifestWithError,
            s => s.AddScoped<SimplePage>(), // TestErrorPage NOT registered
            pageMap: pageMap);

        var ex = new InvalidOperationException("test error for fallback");
        var response = await renderer.RenderErrorAsync(ex);

        // Should fall back to built-in error page
        Assert.Equal(500, response.StatusCode);
        Assert.Contains("Internal Server Error", response.ToString());
    }

    // ─── Cache Control Tests ──────────────────────────────────────────────

    [Fact]
    public void GetCacheControl_ForPage_ReturnsPublicMaxAge()
    {
        var entry = PageEntry("/", "app/page.cs");
        var result = SsrRenderer.GetCacheControl(entry);
        Assert.Equal("public, max-age=3600", result);
    }

    [Fact]
    public void GetCacheControl_ForApi_ReturnsNoCache()
    {
        var entry = new RouteEntry("/api/data", "app/api/data/route.cs", RouteType.Api, RouteSegmentKind.Static);
        var result = SsrRenderer.GetCacheControl(entry);
        Assert.Equal("no-cache", result);
    }

    [Fact]
    public void GetCacheControl_ForError_ReturnsNoStore()
    {
        var entry = new RouteEntry("/_error", "app/error.cs", RouteType.Error, RouteSegmentKind.Static);
        var result = SsrRenderer.GetCacheControl(entry);
        Assert.Equal("no-store", result);
    }

    // ─── RenderContentAsync Tests ─────────────────────────────────────────

    [Fact]
    public async Task RenderContentAsync_ReturnsRawIHtmlContent()
    {
        var manifest = CreateManifest(PageEntry("/", "app/page.cs"));
        var pageMap = new Dictionary<string, Type> { ["app/page.cs"] = typeof(SimplePage) };
        var (renderer, _) = CreateRenderer(manifest,
            s => s.AddScoped<SimplePage>(),
            pageMap: pageMap);

        var content = await renderer.RenderContentAsync("/", CreateContext());

        Assert.NotNull(content);
        var html = content.ToHtml();
        Assert.Contains("<h1>Simple Page</h1>", html);
    }

    [Fact]
    public async Task RenderContentAsync_WithUnknownRoute_ThrowsRenderException()
    {
        var manifest = CreateManifest();
        var (renderer, _) = CreateRenderer(manifest);

        await Assert.ThrowsAsync<NextNet.Exceptions.RenderException>(() =>
            renderer.RenderContentAsync("/missing", CreateContext()));
    }

    // ─── HtmlResponse Tests ───────────────────────────────────────────────

    [Fact]
    public void HtmlResponse_NotFound_Returns404()
    {
        var response = HtmlResponse.NotFound();
        Assert.Equal(404, response.StatusCode);
        Assert.Contains("404", response.ToString());
    }

    [Fact]
    public async Task HtmlResponse_ExecuteAsync_SetsHeadersAndWritesContent()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var content = new RawHtmlContent("<h1>Test</h1>");
        var response = new HtmlResponse(content, 200, "public, max-age=3600");

        await response.ExecuteAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", ctx.Response.ContentType);
        Assert.Equal("public, max-age=3600", ctx.Response.Headers.CacheControl);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("<h1>Test</h1>", body);
    }
}

// Helper layout type for testing layout chains
file sealed class TestWrapLayout : ILayout
{
    public Task<IHtmlContent> Render(IHtmlContent children)
        => Task.FromResult(HtmlHelper.Fragment(
            new RawHtmlContent("<!--test-layout-start-->"),
            children,
            new RawHtmlContent("<!--test-layout-end-->")
        ));
}
