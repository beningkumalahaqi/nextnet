using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RouteManifestTests
{
    [Fact]
    public void Empty_ReturnsEmptyManifest()
    {
        var empty = RouteManifest.Empty;

        Assert.Empty(empty.Routes);
        Assert.Empty(empty.Pages);
        Assert.Empty(empty.Layouts);
        Assert.Empty(empty.ApiRoutes);
        Assert.Null(empty.ErrorPage);
        Assert.Empty(empty.Conflicts);
        Assert.False(empty.HasConflicts);
    }

    [Fact]
    public void Constructor_WithEntries_PopulatesProperties()
    {
        var pages = new List<RouteEntry>
        {
            new("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static),
            new("/contact", "/app/contact/page.cs", RouteType.Page, RouteSegmentKind.Static),
        };

        var layouts = new List<RouteEntry>
        {
            new("/", "/app/layout.cs", RouteType.Layout, RouteSegmentKind.Static),
        };

        var apiRoutes = new List<RouteEntry>
        {
            new("/api/users", "/app/api/users/route.cs", RouteType.Api, RouteSegmentKind.Static),
        };

        var errorPage = new RouteEntry("/", "/app/error.cs", RouteType.Error, RouteSegmentKind.Static);

        var conflicts = new List<RouteConflict>
        {
            new("Test conflict", "/test", Array.Empty<string>(), ConflictSeverity.Warning),
        };

        var allRoutes = new List<RouteEntry>
        {
            pages[0], pages[1], layouts[0], apiRoutes[0], errorPage,
        };

        var manifest = new RouteManifest(allRoutes, pages, layouts, apiRoutes, errorPage, conflicts);

        Assert.Equal(5, manifest.Routes.Count);
        Assert.Equal(2, manifest.Pages.Count);
        Assert.Single(manifest.Layouts);
        Assert.Single(manifest.ApiRoutes);
        Assert.NotNull(manifest.ErrorPage);
        Assert.Single(manifest.Conflicts);
        Assert.True(manifest.HasConflicts);
    }

    [Fact]
    public void RouteEntry_ToString_FormatsCorrectly()
    {
        var entry = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var str = entry.ToString();

        Assert.Contains("Page", str);
        Assert.Contains("/about", str);
    }

    [Fact]
    public void RouteEntry_Equals_ByFilePath()
    {
        var entry1 = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var entry2 = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var entry3 = new RouteEntry("/contact", "/app/contact/page.cs", RouteType.Page, RouteSegmentKind.Static);

        Assert.Equal(entry1, entry2);
        Assert.NotEqual(entry1, entry3);
    }

    [Fact]
    public void RouteEntry_GetHashCode_MatchesFilePath()
    {
        var entry1 = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);
        var entry2 = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);

        Assert.Equal(entry1.GetHashCode(), entry2.GetHashCode());
    }

    [Fact]
    public void RouteEntry_LayoutChain_InitializedEmpty()
    {
        var entry = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);

        Assert.NotNull(entry.LayoutChain);
        Assert.Empty(entry.LayoutChain);
    }

    [Fact]
    public void RouteEntry_LayoutPath_NullByDefault()
    {
        var entry = new RouteEntry("/about", "/app/about/page.cs", RouteType.Page, RouteSegmentKind.Static);

        Assert.Null(entry.LayoutPath);
    }

    [Fact]
    public void RouteManifest_HasConflicts_TrueWhenConflictsExist()
    {
        var manifest = new RouteManifest(
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            Array.Empty<RouteEntry>(),
            null,
            new List<RouteConflict>
            {
                new("Test", "/", Array.Empty<string>(), ConflictSeverity.Warning),
            });

        Assert.True(manifest.HasConflicts);
    }

    [Fact]
    public void RouteConflict_ToString_IncludesSeverityAndPattern()
    {
        var conflict = new RouteConflict(
            "Duplicate route",
            "/about",
            new[] { "/app/a.cs", "/app/b.cs" },
            ConflictSeverity.Error);

        var str = conflict.ToString();
        Assert.Contains("Error", str);
        Assert.Contains("/about", str);
        Assert.Contains("Duplicate route", str);
    }
}
