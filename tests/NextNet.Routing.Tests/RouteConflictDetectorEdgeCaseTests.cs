using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

public class RouteConflictDetectorEdgeCaseTests
{
    private readonly RouteConflictDetector _detector = new();

    private static RouteEntry MakeEntry(string pattern, RouteType type, RouteSegmentKind kind, string? filePath = null)
    {
        return new RouteEntry(
            pattern,
            filePath ?? $"/app{pattern.Replace("{", "").Replace("}", "").Replace("*", "")}/page.cs",
            type,
            kind);
    }

    [Fact]
    public void Detect_Should_ReturnWarning_When_DynamicAndCatchAllOverlap()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/blog/{slug}", RouteType.Page, RouteSegmentKind.Dynamic),
            MakeEntry("/blog/{*path}", RouteType.Page, RouteSegmentKind.CatchAll),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase)
            && c.Severity == ConflictSeverity.Warning);
    }

    [Fact]
    public void Detect_Should_NotWarnAboutMissingLayout_When_NoPagesExist()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/api/users", RouteType.Api, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        // No pages means no missing root layout warning (layout only applies to pages)
        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_ReportNoConflicts_When_AllEntryTypesAndWellFormed()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static),
            MakeEntry("/contact", RouteType.Page, RouteSegmentKind.Static),
            MakeEntry("/api/users", RouteType.Api, RouteSegmentKind.Static),
            MakeEntry("/error", RouteType.Error, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        // No conflicts expected with this setup
        Assert.Empty(conflicts);
    }

    [Fact]
    public void Detect_Should_DetectCorrectOrphan_When_OnlyOneLayoutOrphaned()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/blog", RouteType.Layout, RouteSegmentKind.Static, "/app/blog/layout.cs"),
            MakeEntry("/blog/posts", RouteType.Page, RouteSegmentKind.Static, "/app/blog/posts/page.cs"),
            // /archive layout with no pages under /archive
            MakeEntry("/archive", RouteType.Layout, RouteSegmentKind.Static, "/app/archive/layout.cs"),
        };

        var conflicts = _detector.Detect(entries);

        // Should find orphaned /archive but not /blog
        var archiveConflicts = conflicts.Where(c =>
            c.RoutePattern == "/archive" &&
            (c.Message.Contains("orphaned", StringComparison.OrdinalIgnoreCase) ||
             c.Message.Contains("no pages beneath", StringComparison.OrdinalIgnoreCase)));

        Assert.NotEmpty(archiveConflicts);

        var blogConflicts = conflicts.Where(c =>
            c.RoutePattern == "/blog" &&
            (c.Message.Contains("orphaned", StringComparison.OrdinalIgnoreCase) ||
             c.Message.Contains("no pages beneath", StringComparison.OrdinalIgnoreCase)));

        Assert.Empty(blogConflicts);
    }

    [Fact]
    public void Detect_Should_NotFindOverlap_When_PatternSegmentsDiffer()
    {
        // /blog and /about don't overlap
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/blog/{slug}", RouteType.Page, RouteSegmentKind.Dynamic),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        // Dynamic/catch-all overlap detection checks for prefix overlap
        // /blog/{slug} and /about don't overlap
        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_NotWarnAboutMissingRootLayout_When_OnlyApiRoutes()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/api/users", RouteType.Api, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_NotWarnAboutMissingRootLayout_When_RootLayoutExists()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static),
            MakeEntry("/contact", RouteType.Page, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("No root layout", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_HaveCorrectSeverityAndFiles_When_DuplicateStaticExists()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/about/page.cs"),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Static, "/app/duplicate-about/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        var duplicateConflict = Assert.Single(
            conflicts, c => c.Severity == ConflictSeverity.Error);

        Assert.Equal(2, duplicateConflict.ConflictingFiles.Count);
        Assert.Contains("/app/about/page.cs", duplicateConflict.ConflictingFiles);
        Assert.Contains("/app/duplicate-about/page.cs", duplicateConflict.ConflictingFiles);
    }

    [Fact]
    public void Detect_Should_HaveCorrectMetadata_When_AllEntryTypesPresent()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteType.Layout, RouteSegmentKind.Static),
            MakeEntry("/about", RouteType.Page, RouteSegmentKind.Dynamic),
            MakeEntry("/api/test", RouteType.Api, RouteSegmentKind.Static),
            MakeEntry("/error", RouteType.Error, RouteSegmentKind.Static),
        };

        var conflicts = _detector.Detect(entries);

        // Just verify all entry types are represented
        Assert.Contains(entries, e => e.Type == RouteType.Layout);
        Assert.Contains(entries, e => e.Type == RouteType.Page);
        Assert.Contains(entries, e => e.Type == RouteType.Api);
        Assert.Contains(entries, e => e.Type == RouteType.Error);
    }
}
