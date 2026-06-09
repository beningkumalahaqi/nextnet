using NextNet.Conventions;
using NextNet.Routing.Errors;
using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Detects conflicts and issues among a collection of route entries.
/// </summary>
/// <example>
/// Detecting conflicts in a list of route entries:
/// <code>
/// var detector = new RouteConflictDetector();
/// var entries = new List&lt;RouteEntry&gt; { pageEntry, layoutEntry };
/// var conflicts = detector.Detect(entries);
/// foreach (var conflict in conflicts)
/// {
///     Console.WriteLine($"{conflict.Severity}: {conflict.Message}");
/// }
/// </code>
/// </example>
public sealed class RouteConflictDetector
{
    /// <summary>
    /// Analyzes a list of route entries and returns any conflicts found.
    /// Conflict messages are prefixed with error codes (DS-054 through DS-058).
    /// </summary>
    /// <param name="entries">The route entries to analyze.</param>
    /// <returns>A list of detected conflicts.</returns>
    public IReadOnlyList<RouteConflict> Detect(IReadOnlyList<RouteEntry> entries)
    {
        var conflicts = new List<RouteConflict>();

        if (entries == null || entries.Count == 0)
            return conflicts;

        // Group entries by their route pattern
        var groupedByPattern = entries
            .GroupBy(e => e.RoutePattern, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // 1. Duplicate static routes (Error)
        foreach (var group in groupedByPattern)
        {
            var staticEntries = group
                .Where(e => e.SegmentKind == RouteSegmentKind.Static)
                .ToList();

            if (staticEntries.Count > 1)
            {
                conflicts.Add(new RouteConflict(
                    $"[{RoutingErrorCodes.DuplicateStaticRoute}] Duplicate static route: {staticEntries.Count} files map to the same route pattern '{group.Key}'.",
                    group.Key,
                    staticEntries.Select(e => e.FilePath).ToList(),
                    ConflictSeverity.Error));
            }
        }

        // 2. Static/Dynamic overlap (Warning)
        // A static route and a dynamic route share the same pattern.
        var staticRoutes = entries.Where(e => e.SegmentKind == RouteSegmentKind.Static).ToList();
        var dynamicRoutes = entries.Where(e => e.SegmentKind == RouteSegmentKind.Dynamic).ToList();

        foreach (var staticRoute in staticRoutes)
        {
            foreach (var dynamicRoute in dynamicRoutes)
            {
                if (string.Equals(staticRoute.RoutePattern, dynamicRoute.RoutePattern, StringComparison.OrdinalIgnoreCase))
                {
                    conflicts.Add(new RouteConflict(
                        $"[{RoutingErrorCodes.StaticDynamicOverlap}] Static route '{staticRoute.RoutePattern}' overlaps with dynamic route '{dynamicRoute.RoutePattern}'.",
                        staticRoute.RoutePattern,
                        new[] { staticRoute.FilePath, dynamicRoute.FilePath },
                        ConflictSeverity.Warning));
                }
            }
        }

        // 3. Dynamic/Catch-all overlap (Warning)
        var catchAllRoutes = entries
            .Where(e => e.SegmentKind is RouteSegmentKind.CatchAll or RouteSegmentKind.OptionalCatchAll)
            .ToList();

        foreach (var dynamicRoute in dynamicRoutes)
        {
            foreach (var catchAllRoute in catchAllRoutes)
            {
                // Two patterns overlap if one is a prefix of the other (when replacing parameter segments)
                if (PatternsOverlap(dynamicRoute.RoutePattern, catchAllRoute.RoutePattern))
                {
                    conflicts.Add(new RouteConflict(
                        $"[{RoutingErrorCodes.DynamicCatchAllOverlap}] Dynamic route '{dynamicRoute.RoutePattern}' overlaps with catch-all route '{catchAllRoute.RoutePattern}'.",
                        dynamicRoute.RoutePattern,
                        new[] { dynamicRoute.FilePath, catchAllRoute.FilePath },
                        ConflictSeverity.Warning));
                }
            }
        }

        // 4. Missing root layout (Warning)
        var hasRootLayout = entries.Any(e =>
            e.Type == RouteType.Layout &&
            string.Equals(e.RoutePattern, "/", StringComparison.Ordinal));

        if (!hasRootLayout && entries.Any(e => e.Type == RouteType.Page))
        {
            conflicts.Add(new RouteConflict(
                $"[{RoutingErrorCodes.MissingRootLayout}] No root layout found. Create 'app/layout.cs' to provide a consistent layout for all pages.",
                "/",
                Array.Empty<string>(),
                ConflictSeverity.Warning));
        }

        // 5. Orphaned layout (Warning)
        var layoutPaths = new HashSet<string>(
            entries
                .Where(e => e.Type == RouteType.Layout && !string.Equals(e.RoutePattern, "/", StringComparison.Ordinal))
                .Select(e => e.RoutePattern),
            StringComparer.OrdinalIgnoreCase);

        var pagePrefixes = new HashSet<string>(
            entries
                .Where(e => e.Type == RouteType.Page && e.RoutePattern != "/")
                .SelectMany(e => GetParentPrefixes(e.RoutePattern)),
            StringComparer.OrdinalIgnoreCase);

        foreach (var layoutRoute in layoutPaths)
        {
            if (!pagePrefixes.Contains(layoutRoute))
            {
                var layoutEntry = entries.FirstOrDefault(e =>
                    e.Type == RouteType.Layout &&
                    string.Equals(e.RoutePattern, layoutRoute, StringComparison.OrdinalIgnoreCase));

                if (layoutEntry != null)
                {
                    conflicts.Add(new RouteConflict(
                        $"[{RoutingErrorCodes.OrphanedLayout}] Layout at '{layoutRoute}' has no pages beneath it. Either add pages under this layout or remove the layout file.",
                        layoutRoute,
                        new[] { layoutEntry.FilePath },
                        ConflictSeverity.Warning));
                }
            }
        }

        return conflicts;
    }

    /// <summary>
    /// Determines whether two route patterns overlap, meaning they could match the same URL.
    /// </summary>
    private static bool PatternsOverlap(string patternA, string patternB)
    {
        var segmentsA = patternA.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var segmentsB = patternB.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // If one is prefix of the other, they overlap
        int minLen = Math.Min(segmentsA.Length, segmentsB.Length);
        for (int i = 0; i < minLen; i++)
        {
            var segA = segmentsA[i];
            var segB = segmentsB[i];

            // If both are static and not equal, they don't match
            if (!segA.StartsWith('{') && !segB.StartsWith('{') && !string.Equals(segA, segB, StringComparison.OrdinalIgnoreCase))
                return false;

            // Dynamic/catch-all segments match anything
        }

        // If all compared segments matched, patterns overlap
        return true;
    }

    /// <summary>
    /// Gets all parent directory prefixes for a route pattern.
    /// e.g., "/blog/2024/post" → ["/blog", "/blog/2024"]
    /// </summary>
    private static IEnumerable<string> GetParentPrefixes(string routePattern)
    {
        var segments = routePattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 1; i < segments.Length; i++)
        {
            yield return "/" + string.Join("/", segments.Take(i));
        }
    }
}
