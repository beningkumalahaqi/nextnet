using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// Represents a single discovered route from the file-system scan.
/// </summary>
public sealed class RouteEntry
{
    /// <summary>
    /// Gets the route pattern (e.g. <c>"/blog/{slug}"</c>).
    /// </summary>
    public string RoutePattern { get; }

    /// <summary>
    /// Gets the relative (or full) file path to the source file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the type of the route entry (Page, Layout, Api, Error).
    /// </summary>
    public RouteType Type { get; }

    /// <summary>
    /// Gets the kind of the most dynamic segment in this route pattern.
    /// </summary>
    public RouteSegmentKind SegmentKind { get; }

    /// <summary>
    /// Gets or sets the file path of the nearest layout that applies to this entry.
    /// </summary>
    public string? LayoutPath { get; set; }

    /// <summary>
    /// Gets the ordered chain of layout file paths from nearest to root.
    /// </summary>
    public IReadOnlyList<string> LayoutChain { get; internal set; }

    /// <summary>
    /// Gets or sets the set of HTTP methods supported by this entry.
    /// Populated for <c>RouteType.Api</c> entries during scanning.
    /// Each value is an uppercase HTTP method name (e.g. "GET", "POST", "PUT", "PATCH", "DELETE").
    /// When empty, all standard HTTP methods are assumed to be available.
    /// </summary>
    public ISet<string> HttpMethods { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Initializes a new instance of <see cref="RouteEntry"/>.
    /// </summary>
    public RouteEntry(
        string routePattern,
        string filePath,
        RouteType type,
        RouteSegmentKind segmentKind)
    {
        RoutePattern = routePattern;
        FilePath = filePath;
        Type = type;
        SegmentKind = segmentKind;
        LayoutChain = Array.Empty<string>();
    }

    /// <inheritdoc />
    public override string ToString()
        => $"[{Type}] {RoutePattern} <- {FilePath}";

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is RouteEntry other
           && string.Equals(FilePath, other.FilePath, StringComparison.OrdinalIgnoreCase)
           && string.Equals(RoutePattern, other.RoutePattern, StringComparison.Ordinal);

    /// <inheritdoc />
    public override int GetHashCode()
        => StringComparer.OrdinalIgnoreCase.GetHashCode(FilePath ?? string.Empty);
}
