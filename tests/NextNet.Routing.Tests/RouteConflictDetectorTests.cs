using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RouteConflictDetectorTests
{
    private readonly RouteConflictDetector _detector = new();

    private static RouteEntry MakeEntry(string pattern, RouteType type, RouteSegmentKind kind, string? filePath = null)
    {
        return new RouteEntry(
            pattern,
            filePath ?? $"/app{pattern.Replace("{", "").Replace("}", "")}/page.cs",
            type,
            kind);
    }

    [Fact]
    public void Detect_NoEntries_ReturnsEmpty()
    {
        var conflicts = _detector.Detect(Array.Empty<RouteEntry>());
        Assert.Empty(conflicts);
    }

    [Fact]
    public void Detect_NullEntries_ReturnsEmpty()
    {
        var conflicts = _detector.Detect(null!);
        Assert.Empty(conflicts);
    }

    [Fact]
    public void Detect_UniqueStaticRoutes_NoConflicts()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
            MakeEntry("/contact", RouteType.Page, RouteSegmentKind.Static, "/app/contact/page.cs"),
            MakeEntry("/blog", RouteType.Page, RouteSegmentKind.Static, "/app/blog/page.cs"),
        };

        var conflicts = _detector.Detect(entries);
        Assert.Empty(conflicts);
    }

    [Fact]
    public void Detect_DuplicateStaticRoutes_ReturnsError()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/duplicate/about/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.RoutePattern == "/about"
            && c.Severity == ConflictSeverity.Error
            && c.ConflictingFiles.Count == 2);
    }

    [Fact]
    public void Detect_StaticDynamicOverlap_ReturnsWarning()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Dynamic, "/app/[about]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.RoutePattern == "/about"
            && c.Severity == ConflictSeverity.Warning
            && c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_MissingRootLayout_ReturnsWarning()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.RoutePattern == "/"
            && c.Severity == ConflictSeverity.Warning
            && c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_WithRootLayout_NoMissingLayoutWarning()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_OrphanedLayout_ReturnsWarning()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/blog", RouteType.Layout, RouteSegmentKind.Static, "/app/blog/layout.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.RoutePattern == "/blog"
            && c.Severity == ConflictSeverity.Warning
            && (c.Message.Contains("orphaned", StringComparison.OrdinalIgnoreCase)
                || c.Message.Contains("no pages beneath", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void Detect_NoPages_NoMissingRootLayoutWarning()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/api/users", RouteType.Api, RouteSegmentKind.Static, "/app/api/users/route.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_ConflictToString_FormatsCorrectly()
    {
        var conflict = new RouteConflict(
            "Test conflict",
            "/test",
            new[] { "/app/file1.cs", "/app/file2.cs" },
            ConflictSeverity.Error);

        var str = conflict.ToString();
        Assert.Contains("Error", str);
        Assert.Contains("/test", str);
        Assert.Contains("Test conflict", str);
    }
}
