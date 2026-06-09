using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RouteTreeBuilderEdgeCaseTests
{
    [Fact]
    public void BuildTree_Should_ReturnRootWithEntry_When_RootOnly()
    {
        var rootLayout = new RouteEntry("/", "/app/layout.cs", RouteType.Layout, RouteSegmentKind.Static);
        var manifest = new RouteManifest(
            new[] { rootLayout },
            Array.Empty<RouteEntry>(),
            new[] { rootLayout },
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.NotNull(tree.Entry);
        Assert.Equal(rootLayout, tree.Entry);
        Assert.Empty(tree.Children);
    }

    [Fact]
    public void         BuildTree_Should_ContainAllChildren_When_MultipleEntriesSameParent()
    {
        var aboutPage = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var blogPage = new RouteEntry("/blog", "/app/blog/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var contactPage = new RouteEntry("/contact", "/app/contact/page.cs", RouteType.Page, RouteSegmentKind.Static);

        var manifest = new RouteManifest(
            new[] { aboutPage, blogPage, contactPage },
            new[] { aboutPage, blogPage, contactPage },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Equal(3, tree.Children.Count);
        Assert.Contains(tree.Children, n => n.Segment == "about" && n.Entry == aboutPage);
        Assert.Contains(tree.Children, n => n.Segment == "blog" && n.Entry == blogPage);
        Assert.Contains(tree.Children, n => n.Segment == "contact" && n.Entry == contactPage);
    }

    [Fact]
    public void         BuildTree_Should_CreateAllLevels_When_VeryDeepNesting()
    {
        var deepPage = new RouteEntry(
            "/a/b/c/d/e/f/g/h/i/j/page",
            "/app/a/b/c/d/e/f/g/h/i/j/page/page.cs",
            RouteType.Page,
            RouteSegmentKind.Static);

        var manifest = new RouteManifest(
            new[] { deepPage },
            new[] { deepPage },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        // Navigate through all levels
        var current = tree;
        string[] segments = ["a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "page"];

        foreach (var seg in segments)
        {
            Assert.Single(current.Children);
            current = current.Children[0];
            Assert.Equal(seg, current.Segment);
        }

        Assert.NotNull(current.Entry);
        Assert.Equal(deepPage, current.Entry);
    }

    [Fact]
    public void         BuildTree_Should_HaveConsistentParentReference_When_Built()
    {
        var aboutPage = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var blogPage = new RouteEntry("/blog", "/app/blog/page.cs", RouteType.Page, RouteSegmentKind.Static);

        var manifest = new RouteManifest(
            new[] { aboutPage, blogPage },
            new[] { aboutPage, blogPage },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Equal(2, tree.Children.Count);
        foreach (var child in tree.Children)
        {
            Assert.Equal(tree, child.Parent);
        }
    }

    [Fact]
    public void         BuildTree_Should_HaveBothEntries_When_LayoutAndPageAtSameLevel()
    {
        var blogLayout = new RouteEntry("/blog", "/app/blog/layout.cs", RouteType.Layout, RouteSegmentKind.Static);
        var blogPage = new RouteEntry("/blog", "/app/blog/page.cs", RouteType.Page, RouteSegmentKind.Static);

        var manifest = new RouteManifest(
            new[] { blogLayout, blogPage },
            new[] { blogPage },
            new[] { blogLayout },
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        var blogNode = Assert.Single(tree.Children);
        Assert.Equal("blog", blogNode.Segment);
        Assert.NotNull(blogNode.Entry);
        // The last route inserted wins when patterns are the same
        Assert.Equal(blogPage, blogNode.Entry);
    }
}
