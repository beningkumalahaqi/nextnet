using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Layouts.Tests;

public class ErrorBoundaryRendererTests
{
    // ─── Test doubles ─────────────────────────────────────────────────────

    private sealed class TestPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render() =>
            Task.FromResult(HtmlHelper.Text("Hello from page"));
    }

    private sealed class ThrowingPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render() =>
            throw new InvalidOperationException("Page render failure");
    }

    private sealed class ThrowingLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children) =>
            throw new InvalidOperationException("Layout render failure");
    }

    private sealed class TestErrorPage : IErrorPage
    {
        public Task<IHtmlContent> Render(Exception exception) =>
            Task.FromResult(
                HtmlHelper.Raw($"<div class=\"error\">Error: {exception.Message}</div>"));
    }

    private sealed class ThrowingErrorPage : IErrorPage
    {
        public Task<IHtmlContent> Render(Exception exception) =>
            throw new InvalidOperationException("Error page itself failed");
    }

    private sealed class TestLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children) =>
            Task.FromResult(HtmlHelper.Fragment(
                new RawHtmlContent("<!--layout:start-->"),
                children,
                new RawHtmlContent("<!--layout:end-->")
            ));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static RouteManifest CreateManifest(
        IReadOnlyList<RouteEntry>? pages = null,
        RouteEntry? errorPage = null)
    {
        pages ??= Array.Empty<RouteEntry>();
        return new RouteManifest(
            pages,
            pages.Where(p => p.Type == RouteType.Page).ToList(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            errorPage,
            Array.Empty<RouteConflict>());
    }

    private static RouteEntry PageEntry(string route = "/", string filePath = "app/page.cs")
    {
        return new RouteEntry(route, filePath, RouteType.Page, RouteSegmentKind.Static);
    }

    private static RouteEntry ErrorEntry(string filePath = "app/error.cs")
    {
        return new RouteEntry("/_error", filePath, RouteType.Error, RouteSegmentKind.Static);
    }

    private static ErrorBoundaryRenderer CreateBoundaryRenderer(
        IReadOnlyDictionary<string, Type>? pageMap = null,
        IReadOnlyDictionary<string, Type>? layoutMap = null)
    {
        var resolver = new ConventionRouteComponentResolver(
            pageMap ?? new Dictionary<string, Type>(),
            layoutMap ?? new Dictionary<string, Type>());
        return new ErrorBoundaryRenderer(resolver);
    }

    private static IServiceProvider CreateServiceProvider(
        Action<ServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private static LayoutRenderer CreateLayoutRenderer()
    {
        return new LayoutRenderer();
    }

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public async Task RenderAsync_Should_RenderContent_When_NoError()
    {
        var boundary = CreateBoundaryRenderer();
        var services = CreateServiceProvider();
        var manifest = CreateManifest();

        var result = await boundary.RenderAsync(
            () => Task.FromResult<IHtmlContent>(new RawHtmlContent("OK")),
            Array.Empty<Type>(),
            services,
            manifest);

        Assert.Equal("OK", result.ToHtml());
    }

    [Fact]
    public async Task RenderAsync_Should_ReturnBuiltInError_When_NoErrorPage()
    {
        var boundary = CreateBoundaryRenderer();
        var services = CreateServiceProvider();
        var manifest = CreateManifest();

        var result = await boundary.RenderAsync(
            () => throw new InvalidOperationException("Boom!"),
            Array.Empty<Type>(),
            services,
            manifest);

        var html = result.ToHtml();
        Assert.Contains("Internal Server Error", html);
        Assert.Contains("Boom!", html);
    }

    [Fact]
    public async Task RenderAsync_Should_RenderErrorPage_When_CustomErrorPageExists()
    {
        var pageMap = new Dictionary<string, Type> { ["app/error.cs"] = typeof(TestErrorPage) };
        var boundary = CreateBoundaryRenderer(pageMap: pageMap);
        var services = CreateServiceProvider(s => s.AddScoped<TestErrorPage>());
        var manifest = CreateManifest(errorPage: ErrorEntry());

        var result = await boundary.RenderAsync(
            () => throw new InvalidOperationException("Boom!"),
            Array.Empty<Type>(),
            services,
            manifest);

        var html = result.ToHtml();
        Assert.Contains("Error:", html);
        Assert.Contains("Boom!", html);
    }

    [Fact]
    public async Task RenderAsync_Should_WrapErrorInLayoutChain_When_ErrorPageAndLayoutsExist()
    {
        var pageMap = new Dictionary<string, Type> { ["app/error.cs"] = typeof(TestErrorPage) };
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(TestLayout) };
        var boundary = CreateBoundaryRenderer(pageMap: pageMap, layoutMap: layoutMap);
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<TestErrorPage>();
            s.AddScoped<TestLayout>();
        });
        var manifest = CreateManifest(errorPage: ErrorEntry());
        var layoutRenderer = CreateLayoutRenderer();
        var layoutTypes = new[] { typeof(TestLayout) };

        var result = await boundary.RenderAsync(
            () => throw new InvalidOperationException("Boom!"),
            layoutTypes,
            services,
            manifest,
            layoutRenderer);

        var html = result.ToHtml();
        Assert.Contains("<!--layout:start-->", html);
        Assert.Contains("<!--layout:end-->", html);
        Assert.Contains("Error:", html);
        Assert.Contains("Boom!", html);

        // Layout should be outermost
        Assert.StartsWith("<!--layout:start-->", html);
        Assert.EndsWith("<!--layout:end-->", html);
    }

    [Fact]
    public async Task RenderAsync_Should_FallbackToErrorPage_When_LayoutThrows()
    {
        var pageMap = new Dictionary<string, Type> { ["app/error.cs"] = typeof(TestErrorPage) };
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(ThrowingLayout)
        };
        var boundary = CreateBoundaryRenderer(pageMap: pageMap, layoutMap: layoutMap);
        var services = CreateServiceProvider(s =>
        {
            s.AddScoped<TestErrorPage>();
            s.AddScoped<ThrowingLayout>();
        });
        var manifest = CreateManifest(errorPage: ErrorEntry());
        var layoutRenderer = CreateLayoutRenderer();
        var layoutTypes = new[] { typeof(ThrowingLayout) };

        var result = await boundary.RenderAsync(
            () => Task.FromResult<IHtmlContent>(new RawHtmlContent("OK")),
            layoutTypes,
            services,
            manifest,
            layoutRenderer);

        var html = result.ToHtml();
        Assert.Contains("Error:", html);
        Assert.Contains("Layout render failure", html);
    }

    [Fact]
    public async Task RenderAsync_Should_FallbackToBuiltIn_When_ErrorPageThrows()
    {
        var pageMap = new Dictionary<string, Type> { ["app/error.cs"] = typeof(ThrowingErrorPage) };
        var boundary = CreateBoundaryRenderer(pageMap: pageMap);
        var services = CreateServiceProvider(s => s.AddScoped<ThrowingErrorPage>());
        var manifest = CreateManifest(errorPage: ErrorEntry());

        var result = await boundary.RenderAsync(
            () => throw new InvalidOperationException("Original failure"),
            Array.Empty<Type>(),
            services,
            manifest);

        var html = result.ToHtml();
        Assert.Contains("Internal Server Error", html);
        Assert.Contains("Original failure", html);
    }

    [Fact]
    public async Task RenderAsync_Should_WrapContentInLayouts_When_NoErrors()
    {
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(TestLayout) };
        var boundary = CreateBoundaryRenderer(layoutMap: layoutMap);
        var services = CreateServiceProvider(s => s.AddScoped<TestLayout>());
        var manifest = CreateManifest();
        var layoutRenderer = CreateLayoutRenderer();
        var layoutTypes = new[] { typeof(TestLayout) };

        var result = await boundary.RenderAsync(
            () => Task.FromResult<IHtmlContent>(new RawHtmlContent("PAGE CONTENT")),
            layoutTypes,
            services,
            manifest,
            layoutRenderer);

        var html = result.ToHtml();
        Assert.Equal("<!--layout:start-->PAGE CONTENT<!--layout:end-->", html);
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_RenderContentIsNull()
    {
        var boundary = CreateBoundaryRenderer();
        var services = CreateServiceProvider();
        var manifest = CreateManifest();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RenderAsync(null!, Array.Empty<Type>(), services, manifest));
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_ServiceProviderIsNull()
    {
        var boundary = CreateBoundaryRenderer();
        var manifest = CreateManifest();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RenderAsync(
                () => Task.FromResult<IHtmlContent>(new RawHtmlContent("x")),
                Array.Empty<Type>(),
                null!,
                manifest));
    }

    [Fact]
    public async Task RenderAsync_Should_ThrowArgumentNullException_When_ManifestIsNull()
    {
        var boundary = CreateBoundaryRenderer();
        var services = CreateServiceProvider();

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            boundary.RenderAsync(
                () => Task.FromResult<IHtmlContent>(new RawHtmlContent("x")),
                Array.Empty<Type>(),
                services,
                null!));
    }
}
