using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Parses file paths from the <c>app/</c> directory into route patterns
/// by stripping known suffixes and converting convention segments.
/// </summary>
/// <example>
/// Parsing a dynamic route from a file path:
/// <code>
/// var (pattern, kind) = RoutePatternParser.Parse(
///     "/project/app/blog/[slug]/page.cs",
///     "/project/app");
/// Console.WriteLine(pattern); // "/blog/{slug}"
/// Console.WriteLine(kind);    // RouteSegmentKind.Dynamic
/// </code>
/// </example>
internal static class RoutePatternParser
{
    // The known file suffixes that have special routing meaning.
    private static readonly string[] KnownSuffixes =
    {
        "page.cs",
        "layout.cs",
        "route.cs",
        "error.cs",
    };

    /// <summary>
    /// Parses a file path relative to the application directory into a route pattern
    /// and determines the route segment kind.
    /// </summary>
    /// <param name="filePath">The absolute file path to parse.</param>
    /// <param name="appDir">The absolute path to the application directory (e.g. <c>app/</c>).</param>
    /// <returns>A tuple containing the route pattern and the segment kind.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="appDir"/> is null.</exception>
    public static (string RoutePattern, RouteSegmentKind Kind) Parse(string filePath, string appDir)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(appDir);

        // Normalize directory separator to '/'
        var normalizedAppDir = appDir.Replace('\\', '/').TrimEnd('/');

        // Get relative path from appDir
        var relativePath = filePath.Replace('\\', '/');

        if (relativePath.StartsWith(normalizedAppDir, StringComparison.OrdinalIgnoreCase))
        {
            relativePath = relativePath[(normalizedAppDir.Length + 1)..];
        }
        else
        {
            // If path doesn't start with appDir, try using it as-is
            // This handles cases where the file is already relative
            relativePath = relativePath.TrimStart('/');
        }

        // Remove the known suffix
        var route = RemoveSuffix(relativePath);

        // Normalize: remove trailing slashes, ensure no double slashes
        route = route.Trim('/');

        // Convert bracket notation to route pattern syntax
        // Order matters: handle optional catch-all [[...path]] first, then catch-all [...path], then dynamic [slug]
        route = ConvertBracketNotation(route);

        // Determine the most dynamic segment kind
        var kind = DetermineSegmentKind(route, relativePath);

        // Ensure leading '/'
        if (!route.StartsWith('/'))
        {
            route = "/" + route;
        }

        return (route, kind);
    }

    /// <summary>
    /// Removes the known routing suffix from a relative file path.
    /// </summary>
    private static string RemoveSuffix(string relativePath)
    {
        foreach (var suffix in KnownSuffixes)
        {
            if (relativePath.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            {
                // Remove the suffix and the preceding '/' or directory separator
                var trimmed = relativePath[..^suffix.Length];
                trimmed = trimmed.TrimEnd('/', '\\');
                return trimmed;
            }
        }

        // No known suffix found; return the path minus .cs extension if present
        if (relativePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return relativePath[..^".cs".Length];
        }

        return relativePath;
    }

    /// <summary>
    /// Converts [[...path]] → {{*path}}, [...path] → {*path}, [slug] → {slug}.
    /// Processes optional catch-all first to avoid partial matches.
    /// </summary>
    internal static string ConvertBracketNotation(string segment)
    {
        // Optional catch-all: [[...name]] → {{*name}}
        segment = System.Text.RegularExpressions.Regex.Replace(
            segment,
            @"\[\[\.\.\.(\w+)\]\]",
            "{{*$1}}");

        // Catch-all: [...name] → {*name}
        segment = System.Text.RegularExpressions.Regex.Replace(
            segment,
            @"\[\.\.\.(\w+)\]",
            "{*$1}");

        // Dynamic: [name] → {name}
        segment = System.Text.RegularExpressions.Regex.Replace(
            segment,
            @"\[(\w+)\]",
            "{$1}");

        return segment;
    }

    /// <summary>
    /// Determines the <see cref="RouteSegmentKind"/> for a parsed route pattern.
    /// Inspects the original relative path for bracket notation to decide the kind.
    /// </summary>
    internal static RouteSegmentKind DetermineSegmentKind(string routePattern, string originalRelativePath)
    {
        // Check original path for brackets to determine kind
        if (originalRelativePath.Contains("[[...", StringComparison.Ordinal))
        {
            return RouteSegmentKind.OptionalCatchAll;
        }

        if (originalRelativePath.Contains("[...", StringComparison.Ordinal))
        {
            return RouteSegmentKind.CatchAll;
        }

        if (originalRelativePath.Contains('[', StringComparison.Ordinal))
        {
            return RouteSegmentKind.Dynamic;
        }

        return RouteSegmentKind.Static;
    }
}
