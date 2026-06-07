using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app.blog;
using NextNet.Components;
using NextNet.Rendering.Middleware;
using NextNet.Rendering.Streaming;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app;
using NextNet.Rendering.Tests.Fixtures.SampleApp.app.about;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Rendering.Tests.IntegrationTests;

/// <summary>
/// Full end-to-end integration tests for the SSR pipeline.
/// Exercises route resolution, component instantiation, layout composition, and streaming.
/// </summary>
public class FullPipelineTests
{
    // ─── Helpers ──────────────────────────────────────────────────────────

    private static RouteManifest CreateSampleAppManifest()
    {
        var homeEntry = new RouteEntry("/", "app/page.cs", RouteType.Page, RouteSegmentKind.Static);
        homeEntry.LayoutChain = new[] { "app/layout.cs" };

        var aboutEntry = new RouteEntry("/about", "app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        aboutEntry.LayoutChain = new[] { "app/layout.cs" };

        var blogEntry = new RouteEntry("/blog", "app/blog/page.cs", RouteType.Page, RouteSegmentKind.Static);
        blogEntry.LayoutChain = new[] { "app/blog/layout.cs", "app/layout.cs" };

        var pages = new[] { homeEntry, aboutEntry, blogEntry };

        return new RouteManifest(
            pages,
            pages,
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());
    }

    private static (SsrRenderer Ssr, StreamingHtmlRenderer Streaming, IServiceProvider Services)
        CreateSampleAppPipeline(Action<ServiceCollection>? extraServices = null)
    {
        var manifest = CreateSampleAppManifest();
        var services = new ServiceCollection();

        // Register components
        services.AddScoped<HomePage>();
        services.AddScoped<AboutPage>();
        services.AddScoped<SampleBlogPage>();
        services.AddScoped<RootLayout>();
        services.AddScoped<BlogLayout>();

        extraServices?.Invoke(services);
        var sp = services.BuildServiceProvider();

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(HomePage),
            ["app/about/page.cs"] = typeof(AboutPage),
            ["app/blog/page.cs"] = typeof(SampleBlogPage),
        };

        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(RootLayout),
            ["app/blog/layout.cs"] = typeof(BlogLayout),
        };

        var resolver = new ConventionRouteComponentResolver(pageMap, layoutMap);
        var options = new SsrOptions
        {
            Streaming = true,
            BufferSize = 4096,
            RenderTimeout = TimeSpan.FromSeconds(10)
        };

        var ssr = new SsrRenderer(sp, manifest, options, resolver);
        var streaming = new StreamingHtmlRenderer(ssr, options);

        return (ssr, streaming, sp);
    }

    private static ComponentContext CreateContext()
    {
        return new ComponentContext(new DefaultHttpContext());
    }

    // ─── Full SSR Pipeline Tests ──────────────────────────────────────────

    [Fact]
    public async Task FullPipeline_HomePage_RendersCompleteHtmlDocument()
    {
        var (ssr, _, _) = CreateSampleAppPipeline();
        var context = CreateContext();

        var response = await ssr.RenderAsync("/", context);

        Assert.Equal(200, response.StatusCode);
        var html = response.ToString();

        // Document structure
        Assert.StartsWith("<!DOCTYPE html>", html);
        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<title>Sample App</title>", html);
        Assert.Contains("</head>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</body>", html);
        Assert.Contains("</html>", html);

        // Navigation
        Assert.Contains("<nav>", html);
        Assert.Contains("<a href=\"/\">Home</a>", html);
        Assert.Contains("<a href=\"/about\">About</a>", html);

        // Page content
        Assert.Contains("<h1>Welcome to NextNet</h1>", html);
        Assert.Contains("This is the home page rendered with SSR.", html);

        // Raw HTML bypass (page includes Raw content)
        Assert.Contains("Raw <b>HTML</b> content here.", html);

        // Footer
        Assert.Contains("<footer>", html);
        Assert.Contains("Sample App", html);
    }

    [Fact]
    public async Task FullPipeline_AboutPage_RendersCorrectContent()
    {
        var (ssr, _, _) = CreateSampleAppPipeline();
        var context = CreateContext();

        var response = await ssr.RenderAsync("/about", context);

        Assert.Equal(200, response.StatusCode);
        var html = response.ToString();

        Assert.Contains("<h1>About NextNet</h1>", html);
        Assert.Contains("<li>Server-side rendering</li>", html);
        Assert.Contains("<li>File-based routing</li>", html);
        Assert.Contains("<li>Streaming HTML</li>", html);
    }

    [Fact]
    public async Task FullPipeline_WithMiddleware_ResolvesRoutesAndRenders()
    {
        var (ssr, streaming, sp) = CreateSampleAppPipeline();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/";
        ctx.Request.Headers.Accept = "text/html";
        ctx.Response.Body = new MemoryStream();

        var options = new SsrOptions { Streaming = false };
        var middleware = new SsrMiddleware(
            _ => Task.CompletedTask, // next middleware
            ssr,
            streaming,
            options);

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode);
        Assert.Equal("text/html; charset=utf-8", ctx.Response.ContentType);

        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(ctx.Response.Body).ReadToEnd();
        Assert.Contains("Welcome to NextNet", body);
        Assert.Contains("<!DOCTYPE html>", body);
    }

    [Fact]
    public async Task FullPipeline_Middleware_WithNonHtmlRequest_PassesThrough()
    {
        var (ssr, streaming, sp) = CreateSampleAppPipeline();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/";
        ctx.Request.Headers.Accept = "application/json";
        ctx.Response.Body = new MemoryStream();

        var passedThrough = false;
        var middleware = new SsrMiddleware(
            _ =>
            {
                passedThrough = true;
                return Task.CompletedTask;
            },
            ssr,
            streaming);

        await middleware.InvokeAsync(ctx);

        Assert.True(passedThrough, "Middleware should pass through for non-HTML requests");
    }

    [Fact]
    public async Task FullPipeline_Middleware_WithUnknownRoute_PassesThrough()
    {
        var (ssr, streaming, sp) = CreateSampleAppPipeline();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path = "/nonexistent";
        ctx.Response.Body = new MemoryStream();

        var passedThrough = false;
        var middleware = new SsrMiddleware(
            _ =>
            {
                passedThrough = true;
                return Task.CompletedTask;
            },
            ssr,
            streaming);

        await middleware.InvokeAsync(ctx);

        Assert.True(passedThrough, "Middleware should pass through for unknown routes");
    }

    [Fact]
    public async Task FullPipeline_Middleware_WithPostMethod_PassesThrough()
    {
        var (ssr, streaming, sp) = CreateSampleAppPipeline();
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path = "/";
        ctx.Response.Body = new MemoryStream();

        var passedThrough = false;
        var middleware = new SsrMiddleware(
            _ =>
            {
                passedThrough = true;
                return Task.CompletedTask;
            },
            ssr,
            streaming);

        await middleware.InvokeAsync(ctx);

        Assert.True(passedThrough, "Middleware should pass through for POST requests");
    }

    [Fact]
    public async Task FullPipeline_Streaming_ProducesProgressiveChunks()
    {
        var (_, streaming, _) = CreateSampleAppPipeline();
        var context = CreateContext();

        var chunks = new List<string>();
        await foreach (var chunk in streaming.RenderAsyncEnumerable("/", context))
        {
            chunks.Add(chunk);
        }

        Assert.True(chunks.Count > 0, "Should produce at least one chunk");
        var combined = string.Join("", chunks);
        Assert.Contains("<!DOCTYPE html>", combined);
        Assert.Contains("Welcome to NextNet", combined);
    }

    [Fact]
    public async Task FullPipeline_BlogPage_WithNestedLayout_ComposesCorrectly()
    {
        var (ssr, _, _) = CreateSampleAppPipeline();
        var context = CreateContext();

        var response = await ssr.RenderAsync("/blog", context);

        Assert.Equal(200, response.StatusCode);
        var html = response.ToString();

        // Root layout
        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("<!DOCTYPE html>", html);

        // Blog layout (nested)
        Assert.Contains("class=\"blog-layout\"", html);
        Assert.Contains("class=\"sidebar\"", html);
        Assert.Contains("Blog Sidebar", html);
        Assert.Contains("class=\"blog-content\"", html);

        // Page content
        Assert.Contains("Blog Page", html);
    }

    [Fact]
    public async Task FullPipeline_Response_ContainsValidCacheHeaders()
    {
        var (ssr, _, _) = CreateSampleAppPipeline();
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var response = await ssr.RenderAsync("/", CreateContext());
        await response.ExecuteAsync(ctx);

        // Cache-Control should be set for static pages
        Assert.Contains("public", ctx.Response.Headers.CacheControl.ToString());
    }

    [Fact]
    public async Task FullPipeline_ErrorInPage_RendersErrorResponse()
    {
        // Add a failing route
        var failingEntry = new RouteEntry("/fail", "app/fail/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var manifest = CreateSampleAppManifest();
        var allPages = manifest.Pages.Concat(new[] { failingEntry }).ToList();
        var failManifest = new RouteManifest(
            allPages, allPages, manifest.Layouts, manifest.ApiRoutes, null, manifest.Conflicts);

        var services = new ServiceCollection();
        services.AddScoped<HomePage>();
        services.AddScoped<AboutPage>();
        services.AddScoped<SampleBlogPage>();
        services.AddScoped<RootLayout>();
        services.AddScoped<BlogLayout>();
        services.AddScoped<FailingPage>();
        var sp = services.BuildServiceProvider();

        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(HomePage),
            ["app/about/page.cs"] = typeof(AboutPage),
            ["app/blog/page.cs"] = typeof(SampleBlogPage),
            ["app/fail/page.cs"] = typeof(FailingPage),
        };
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(RootLayout),
            ["app/blog/layout.cs"] = typeof(BlogLayout),
        };

        var resolver = new ConventionRouteComponentResolver(pageMap, layoutMap);
        var ssr = new SsrRenderer(sp, failManifest, new SsrOptions { Streaming = false }, resolver);

        var response = await ssr.RenderAsync("/fail", CreateContext());

        Assert.Equal(500, response.StatusCode);
        Assert.Contains("Error", response.ToString());
    }
}

// ─── Additional test types ───────────────────────────────────────────────

file sealed class SampleBlogPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public Task<IHtmlContent> Render() =>
        Task.FromResult(HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("Blog Page")),
            HtmlHelper.Element("p", content: HtmlHelper.Text("Welcome to the blog section."))
        ));
}

file sealed class FailingPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();

    public Task<IHtmlContent> Render() =>
        throw new InvalidOperationException("Intentional failure for integration test");
}
