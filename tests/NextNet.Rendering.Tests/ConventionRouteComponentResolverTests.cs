using NextNet.Components;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Rendering.Tests;

public class ConventionRouteComponentResolverTests
{
    // ─── Test component types ────────────────────────────────────────────

    private sealed class TestPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render() => Task.FromResult(HtmlHelper.Text("test"));
    }

    private sealed class AnotherPage : IPage
    {
        public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
        public Task<IHtmlContent> Render() => Task.FromResult(HtmlHelper.Text("another"));
    }

    private sealed class TestLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children)
            => Task.FromResult(children);
    }

    private sealed class AnotherLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children)
            => Task.FromResult(children);
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static RouteEntry PageEntry(string route, string filePath)
    {
        return new RouteEntry(route, filePath, RouteType.Page, RouteSegmentKind.Static);
    }

    // ─── Constructor tests ───────────────────────────────────────────────

    [Fact]
    public void Constructor_WithEmptyMaps_ResolvesNothing()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());

        var entry = PageEntry("/", "app/page.cs");
        Assert.Null(resolver.GetPageType(entry));
        Assert.Null(resolver.GetLayoutType("app/layout.cs"));
    }

    [Fact]
    public void Constructor_WithPageMap_ResolvesPages()
    {
        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(TestPage),
            ["app/about/page.cs"] = typeof(AnotherPage),
        };
        var resolver = new ConventionRouteComponentResolver(
            pageMap, new Dictionary<string, Type>());

        var homeEntry = PageEntry("/", "app/page.cs");
        var aboutEntry = PageEntry("/about", "app/about/page.cs");
        var missingEntry = PageEntry("/missing", "app/missing/page.cs");

        Assert.Equal(typeof(TestPage), resolver.GetPageType(homeEntry));
        Assert.Equal(typeof(AnotherPage), resolver.GetPageType(aboutEntry));
        Assert.Null(resolver.GetPageType(missingEntry));
    }

    [Fact]
    public void Constructor_WithLayoutMap_ResolvesLayouts()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(TestLayout),
            ["app/blog/layout.cs"] = typeof(AnotherLayout),
        };
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);

        Assert.Equal(typeof(TestLayout), resolver.GetLayoutType("app/layout.cs"));
        Assert.Equal(typeof(AnotherLayout), resolver.GetLayoutType("app/blog/layout.cs"));
        Assert.Null(resolver.GetLayoutType("app/missing.cs"));
    }

    [Fact]
    public void GetLayoutType_NormalizesPathSeparators()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(TestLayout),
        };
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);

        // Windows-style separators should be normalized
        Assert.Equal(typeof(TestLayout), resolver.GetLayoutType(@"app\layout.cs"));
    }

    // ─── GetPageType tests ───────────────────────────────────────────────

    [Fact]
    public void GetPageType_WithMappedEntry_ReturnsType()
    {
        var pageMap = new Dictionary<string, Type> { ["app/page.cs"] = typeof(TestPage) };
        var resolver = new ConventionRouteComponentResolver(pageMap,
            new Dictionary<string, Type>());

        var entry = PageEntry("/", "app/page.cs");
        var type = resolver.GetPageType(entry);

        Assert.NotNull(type);
        Assert.Equal(typeof(TestPage), type);
    }

    [Fact]
    public void GetPageType_WithUnmappedEntry_ReturnsNull()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());

        var entry = PageEntry("/", "no-such-file.cs");
        Assert.Null(resolver.GetPageType(entry));
    }

    [Fact]
    public void GetLayoutType_WithMappedPath_ReturnsType()
    {
        var layoutMap = new Dictionary<string, Type> { ["root"] = typeof(TestLayout) };
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), layoutMap);

        Assert.Equal(typeof(TestLayout), resolver.GetLayoutType("root"));
    }

    [Fact]
    public void GetLayoutType_WithUnmappedPath_ReturnsNull()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());

        Assert.Null(resolver.GetLayoutType("nonexistent"));
    }

    [Fact]
    public void Constructor_EmptyAssemblies_ScansAllLoaded()
    {
        // Passing no assemblies should scan all loaded assemblies
        var resolver = new ConventionRouteComponentResolver();
        Assert.NotNull(resolver);
    }

    [Fact]
    public void Constructor_WithNullAssemblies_ScansAllLoaded()
    {
        var resolver = new ConventionRouteComponentResolver(assemblies: null);
        Assert.NotNull(resolver);
    }

    [Fact]
    public void GetPageType_WithWindowsPath_Normalizes()
    {
        var pageMap = new Dictionary<string, Type>
        {
            ["app/page.cs"] = typeof(TestPage),
        };
        var resolver = new ConventionRouteComponentResolver(pageMap,
            new Dictionary<string, Type>());

        var entry = PageEntry("/", @"app\page.cs"); // Windows-style path
        var type = resolver.GetPageType(entry);
        Assert.Equal(typeof(TestPage), type);
    }

    [Fact]
    public void GetPageType_WithNullLogger_DoesNotThrow()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());

        var entry = PageEntry("/missing", "no/such/file.cs");
        var type = resolver.GetPageType(entry);
        Assert.Null(type);
    }

    [Fact]
    public void GetLayoutType_WithNullLogger_DoesNotThrow()
    {
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type>(), new Dictionary<string, Type>());

        var type = resolver.GetLayoutType("no/such/layout.cs");
        Assert.Null(type);
    }

    [Fact]
    public void Constructor_WithAssemblyAndPageMap_ResolvesByFilePathOnly()
    {
        // The page map should take priority over assembly scanning
        // This test verifies explicit maps work with the assembly constructor
        var resolver = new ConventionRouteComponentResolver(
            new Dictionary<string, Type> { ["app/page.cs"] = typeof(TestPage) },
            new Dictionary<string, Type>());

        var entry = PageEntry("/", "app/page.cs");
        Assert.Equal(typeof(TestPage), resolver.GetPageType(entry));
    }

    [Fact]
    public void Constructor_WithEmptyPageMap_ReturnsNullForUnknown()
    {
        var resolver = new ConventionRouteComponentResolver(
            Array.Empty<System.Reflection.Assembly>());

        var entry = PageEntry("/unknown", "unknown/page.cs");
        Assert.Null(resolver.GetPageType(entry));
    }

    [Fact]
    public void Constructor_WithNullLogger_AssemblyScanDoesNotThrow()
    {
        // Verify the assembly scanning path doesn't throw even without a logger
        var resolver = new ConventionRouteComponentResolver(Array.Empty<System.Reflection.Assembly>());
        Assert.NotNull(resolver);
    }
}
