using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RouteTreeBuilderTests
{
    [Fact]
    public void BuildTree_EmptyManifest_ReturnsRootNode()
    {
        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(RouteManifest.Empty);

        Assert.NotNull(tree);
        Assert.Equal("/", tree.Segment);
        Assert.Null(tree.Entry);
        Assert.Null(tree.Parent);
        Assert.Empty(tree.Children);
    }

    [Fact]
    public void BuildTree_NullManifest_Throws()
    {
        var builder = new RouteTreeBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.BuildTree(null!));
    }

    [Fact]
    public void BuildTree_SinglePage_CreatesCorrectTree()
    {
        var page = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var manifest = new RouteManifest(
            new[] { page },
            new[] { page },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Single(tree.Children);
        var aboutNode = tree.Children[0];
        Assert.Equal("about", aboutNode.Segment);
        Assert.Equal(page, aboutNode.Entry);
        Assert.Equal(tree, aboutNode.Parent);
        Assert.Equal("/about", aboutNode.RoutePattern);
        Assert.Empty(aboutNode.Children);
    }

    [Fact]
    public void BuildTree_MultiplePages_CreatesCorrectTree()
    {
        var about = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var contact = new RouteEntry("/contact", "/app/contact/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var blog = new RouteEntry("/blog/{slug}", "/app/blog/[slug]/page.cs", RouteType.Page, RouteSegmentKind.Dynamic);

        var manifest = new RouteManifest(
            new[] { about, contact, blog },
            new[] { about, contact, blog },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Equal(3, tree.Children.Count);
    }

    [Fact]
    public void BuildTree_WithRootLayout_RootNodeHasEntry()
    {
        var rootLayout = new RouteEntry("/", "/app/layout.cs", RouteType.Layout, RouteSegmentKind.Static);
        var about = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);

        var manifest = new RouteManifest(
            new[] { rootLayout, about },
            new[] { about },
            new[] { rootLayout },
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.NotNull(tree.Entry);
        Assert.Equal(rootLayout, tree.Entry);
    }

    [Fact]
    public void BuildTree_DeepNestedRoutes_CreatesHierarchy()
    {
        var blog2024slug = new RouteEntry(
            "/blog/2024/{slug}",
            "/app/blog/2024/[slug]/page.cs",
            RouteType.Page,
            RouteSegmentKind.Dynamic);

        var manifest = new RouteManifest(
            new[] { blog2024slug },
            new[] { blog2024slug },
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Single(tree.Children); // blog
        var blogNode = tree.Children[0];
        Assert.Equal("blog", blogNode.Segment);
        Assert.Null(blogNode.Entry); // intermediate node, no page

        Assert.Single(blogNode.Children); // 2024
        var yearNode = blogNode.Children[0];
        Assert.Equal("2024", yearNode.Segment);
        Assert.Null(yearNode.Entry);

        Assert.Single(yearNode.Children); // {slug}
        var slugNode = yearNode.Children[0];
        Assert.Equal("{slug}", slugNode.Segment);
        Assert.NotNull(slugNode.Entry);
        Assert.Equal(blog2024slug, slugNode.Entry);
    }

    [Fact]
    public void BuildTree_TreeNodeToString_FormatsCorrectly()
    {
        var node = new RouteTreeNode(
            "{slug}",
            "/blog/{slug}",
            null,
            null,
            new List<RouteTreeNode>());

        var str = node.ToString();
        Assert.Contains("{slug}", str);
        Assert.Contains("/blog/{slug}", str);
    }

    [Fact]
    public void BuildTree_IntermediateNodeWithEntry_KeepsEntry()
    {
        // A layout at /blog and a page at /blog/[slug]
        var blogLayout = new RouteEntry("/blog", "/app/blog/layout.cs", RouteType.Layout, RouteSegmentKind.Static);
        var blogPage = new RouteEntry(
            "/blog/{slug}",
            "/app/blog/[slug]/page.cs",
            RouteType.Page,
            RouteSegmentKind.Dynamic);

        var manifest = new RouteManifest(
            new[] { blogLayout, blogPage },
            new[] { blogPage },
            new[] { blogLayout },
            Array.Empty<RouteEntry>(),
            null,
            Array.Empty<RouteConflict>());

        var builder = new RouteTreeBuilder();
        var tree = builder.BuildTree(manifest);

        Assert.Single(tree.Children);
        var blogNode = tree.Children[0];
        Assert.Equal("blog", blogNode.Segment);
        Assert.NotNull(blogNode.Entry);
        Assert.Equal(blogLayout, blogNode.Entry);
        Assert.Equal("/blog", blogNode.RoutePattern);

        Assert.Single(blogNode.Children);
        var slugNode = blogNode.Children[0];
        Assert.Equal("{slug}", slugNode.Segment);
        Assert.NotNull(slugNode.Entry);
        Assert.Equal(blogPage, slugNode.Entry);
    }
}
