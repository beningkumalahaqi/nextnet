using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Rendering;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Layouts.Tests;

public class LayoutChainResolverTests
{
    // ─── Test doubles ─────────────────────────────────────────────────────

    private sealed class RootLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children)
            => Task.FromResult(children);
    }

    private sealed class NestedLayout : ILayout
    {
        public Task<IHtmlContent> Render(IHtmlContent children)
            => Task.FromResult(children);
    }

    private sealed class NotALayout
    {
        // Does not implement ILayout
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private static RouteEntry MakeEntry(
        string routePattern = "/",
        string filePath = "app/page.cs",
        string[]? layoutChain = null)
    {
        var entry = new RouteEntry(routePattern, filePath, RouteType.Page, RouteSegmentKind.Static);
        entry.LayoutChain = (IReadOnlyList<string>)(layoutChain ?? Array.Empty<string>());
        return entry;
    }

    private static LayoutChainResolver CreateResolver(
        IReadOnlyDictionary<string, Type>? layoutMap = null,
        IReadOnlyDictionary<string, Type>? pageMap = null)
    {
        var map = layoutMap ?? new Dictionary<string, Type>();
        var pMap = pageMap ?? new Dictionary<string, Type>();
        var resolver = new ConventionRouteComponentResolver(pMap, map);
        return new LayoutChainResolver(resolver);
    }

    // ─── Tests ────────────────────────────────────────────────────────────

    [Fact]
    public void ResolveChain_WithEmptyChain_ReturnsEmptyList()
    {
        var resolver = CreateResolver();
        var entry = MakeEntry(layoutChain: Array.Empty<string>());

        var result = resolver.ResolveChain(entry);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ResolveChain_WithSingleLayout_ReturnsSingleType()
    {
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(RootLayout) };
        var resolver = CreateResolver(layoutMap);
        var entry = MakeEntry(layoutChain: new[] { "app/layout.cs" });

        var result = resolver.ResolveChain(entry);

        Assert.Single(result);
        Assert.Equal(typeof(RootLayout), result[0]);
    }

    [Fact]
    public void ResolveChain_WithNestedLayouts_ReturnsOrderedTypes()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(RootLayout),
            ["app/blog/layout.cs"] = typeof(NestedLayout)
        };
        var resolver = CreateResolver(layoutMap);
        // LayoutChain order: innermost first → outermost last
        var entry = MakeEntry(layoutChain: new[] { "app/blog/layout.cs", "app/layout.cs" });

        var result = resolver.ResolveChain(entry);

        Assert.Equal(2, result.Count);
        Assert.Equal(typeof(NestedLayout), result[0]); // innermost
        Assert.Equal(typeof(RootLayout), result[1]);   // outermost
    }

    [Fact]
    public void ResolveChain_WithThreeLevels_ReturnsAllTypes()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(RootLayout),
            ["app/blog/layout.cs"] = typeof(NestedLayout),
            ["app/blog/posts/layout.cs"] = typeof(NestedLayout)
        };
        var resolver = CreateResolver(layoutMap);
        var entry = MakeEntry(
            layoutChain: new[] { "app/blog/posts/layout.cs", "app/blog/layout.cs", "app/layout.cs" });

        var result = resolver.ResolveChain(entry);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void ResolveChain_WithUnresolvableLayout_ThrowsRenderException()
    {
        var resolver = CreateResolver(); // no layouts registered
        var entry = MakeEntry(layoutChain: new[] { "app/layout.cs" });

        var ex = Assert.Throws<RenderException>(() => resolver.ResolveChain(entry));
        Assert.Contains("Cannot resolve layout type", ex.Message);
        Assert.Contains("app/layout.cs", ex.Message);
    }

    [Fact]
    public void ResolveChain_WithNonLayoutType_ThrowsRenderException()
    {
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(NotALayout) };
        var resolver = CreateResolver(layoutMap);
        var entry = MakeEntry(layoutChain: new[] { "app/layout.cs" });

        var ex = Assert.Throws<RenderException>(() => resolver.ResolveChain(entry));
        Assert.Contains("does not implement ILayout", ex.Message);
    }

    [Fact]
    public void ResolveChain_WithNullEntry_ThrowsArgumentNullException()
    {
        var resolver = CreateResolver();

        Assert.Throws<ArgumentNullException>(() => resolver.ResolveChain(null!));
    }

    [Fact]
    public void ResolveChain_CachesResults()
    {
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(RootLayout) };
        var resolver = CreateResolver(layoutMap);
        var entry = MakeEntry(layoutChain: new[] { "app/layout.cs" });

        var first = resolver.ResolveChain(entry);
        var second = resolver.ResolveChain(entry);

        Assert.Same(first, second); // same cached instance
    }

    [Fact]
    public void ClearCache_RemovesCachedEntries()
    {
        var layoutMap = new Dictionary<string, Type> { ["app/layout.cs"] = typeof(RootLayout) };
        var resolver = CreateResolver(layoutMap);
        var entry = MakeEntry(layoutChain: new[] { "app/layout.cs" });

        var first = resolver.ResolveChain(entry);
        resolver.ClearCache();
        var second = resolver.ResolveChain(entry);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second); // different instances after cache clear
    }

    [Fact]
    public void ResolveChain_DifferentRoutes_DifferentCacheEntries()
    {
        var layoutMap = new Dictionary<string, Type>
        {
            ["app/layout.cs"] = typeof(RootLayout),
            ["app/blog/layout.cs"] = typeof(NestedLayout)
        };
        var resolver = CreateResolver(layoutMap);

        var entry1 = MakeEntry("/", layoutChain: new[] { "app/layout.cs" });
        var entry2 = MakeEntry("/blog", layoutChain: new[] { "app/blog/layout.cs", "app/layout.cs" });

        var chain1 = resolver.ResolveChain(entry1);
        var chain2 = resolver.ResolveChain(entry2);

        Assert.Single(chain1);
        Assert.Equal(2, chain2.Count);
    }

    [Fact]
    public void ResolveChain_ExceedingMaxDepth_ThrowsRenderException()
    {
        var layoutMap = new Dictionary<string, Type>();
        var types = new List<Type>();
        for (int i = 0; i < 15; i++)
        {
            var key = $"app/layout{i}.cs";
            layoutMap[key] = typeof(RootLayout);
            types.Add(typeof(RootLayout));
        }

        var resolver = CreateResolver(layoutMap);
        resolver.MaxDepth = 10;
        var entry = MakeEntry(
            layoutChain: Enumerable.Range(0, 15).Select(i => $"app/layout{i}.cs").ToArray());

        var ex = Assert.Throws<RenderException>(() => resolver.ResolveChain(entry));
        Assert.Contains("exceeds the maximum depth", ex.Message);
    }
}
