using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Routing.Tests;

/// <summary>
/// Tests the pattern overlap detection logic in <see cref="RouteConflictDetector"/>.
/// These tests exercise the internal PatternsOverlap method indirectly through Detect().
/// </summary>
public class RouteConflictDetectorPatternOverlapTests
{
    private readonly RouteConflictDetector _detector = new();

    private static RouteEntry MakeEntry(string pattern, RouteSegmentKind kind, string filePath)
    {
        return new RouteEntry(pattern, filePath, RouteType.Page, kind);
    }

    [Fact]
    public void Detect_Should_FindOverlap_When_DynamicAndCatchAllSharePrefix()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/blog/{slug}", RouteSegmentKind.Dynamic, "/app/blog/[slug]/page.cs"),
            MakeEntry("/blog/{*path}", RouteSegmentKind.CatchAll, "/app/blog/[...path]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase)
            && c.ConflictingFiles.Count == 2);
    }

    [Fact]
    public void Detect_Should_NotFindOverlap_When_DynamicAndCatchAllDiffer()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/blog/{slug}", RouteSegmentKind.Dynamic, "/app/blog/[slug]/page.cs"),
            MakeEntry("/docs/{*path}", RouteSegmentKind.CatchAll, "/app/docs/[...path]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_FindOverlap_When_OptionalCatchAllAndDynamicSharePrefix()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/products/{id}", RouteSegmentKind.Dynamic, "/app/products/[id]/page.cs"),
            MakeEntry("/products/{{*path}}", RouteSegmentKind.OptionalCatchAll, "/app/products/[[...path]]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_FindOverlap_When_MultipleDynamicAndCatchAllOverlap()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/{category}/{product}", RouteSegmentKind.Dynamic, "/app/[category]/[product]/page.cs"),
            MakeEntry("/{category}/{*rest}", RouteSegmentKind.CatchAll, "/app/[category]/[...rest]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_NotFindOverlap_When_PatternsCompletelyDifferent()
    {
        var entries = new List<RouteEntry>
        {
            MakeEntry("/", RouteSegmentKind.Static, "/app/layout.cs"),
            MakeEntry("/foo/{slug}", RouteSegmentKind.Dynamic, "/app/foo/[slug]/page.cs"),
            MakeEntry("/bar/{*path}", RouteSegmentKind.CatchAll, "/app/bar/[...path]/page.cs"),
        };

        var conflicts = _detector.Detect(entries);

        // /foo/{slug} and /bar/{*path} don't overlap
        Assert.DoesNotContain(conflicts, c =>
            c.Message.Contains("overlap", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Detect_Should_ReportAllConflicts_When_MultipleTypesPresent()
    {
        var entries = new List<RouteEntry>
        {
            // Duplicate static routes (error)
            MakeEntry("/about", RouteSegmentKind.Static, "/app/about/page.cs"),
            MakeEntry("/about", RouteSegmentKind.Static, "/app/about/alt/page.cs"),
            // Static/dynamic overlap (warning)
            MakeEntry("/about", RouteSegmentKind.Dynamic, "/app/[about]/page.cs"),
            // No root layout (warning) - no root layout.cs
        };

        var conflicts = _detector.Detect(entries);

        Assert.Contains(conflicts, c => c.Severity == ConflictSeverity.Error);
        Assert.Contains(conflicts, c => c.Severity == ConflictSeverity.Warning);
        Assert.True(conflicts.Count >= 2);
    }
}
