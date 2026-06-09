using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// An immutable snapshot of the discovered routes, pages, layouts, API routes,
/// error pages, and any conflicts found during scanning.
/// </summary>
/// <param name="Routes">All discovered route entries across all types.</param>
/// <param name="Pages">The discovered page entries (<see cref="RouteType.Page"/>).</param>
/// <param name="Layouts">The discovered layout entries (<see cref="RouteType.Layout"/>).</param>
/// <param name="ApiRoutes">The discovered API route entries (<see cref="RouteType.Api"/>).</param>
/// <param name="ErrorPage">The error page entry, if one was found (<see cref="RouteType.Error"/>).</param>
/// <param name="Conflicts">The list of conflicts discovered between routes.</param>
/// <example>
/// Creating a manifest with a single page and no conflicts:
/// <code>
/// var manifest = new RouteManifest(
///     routes: new[] { pageEntry },
///     pages: new[] { pageEntry },
///     layouts: Array.Empty&lt;RouteEntry&gt;(),
///     apiRoutes: Array.Empty&lt;RouteEntry&gt;(),
///     errorPage: null,
///     conflicts: Array.Empty&lt;RouteConflict&gt;());
/// </code>
/// </example>
public sealed record RouteManifest(
    IReadOnlyList<RouteEntry> Routes,
    IReadOnlyList<RouteEntry> Pages,
    IReadOnlyList<RouteEntry> Layouts,
    IReadOnlyList<RouteEntry> ApiRoutes,
    RouteEntry? ErrorPage,
    IReadOnlyList<RouteConflict> Conflicts)
{
    /// <summary>
    /// Gets a value indicating whether the manifest contains any conflicts.
    /// </summary>
    public bool HasConflicts => Conflicts.Count > 0;

    /// <summary>
    /// Returns an empty <see cref="RouteManifest"/> with no routes and no conflicts.
    /// </summary>
    public static RouteManifest Empty { get; } = new RouteManifest(
        Array.Empty<RouteEntry>(),
        Array.Empty<RouteEntry>(),
        Array.Empty<RouteEntry>(),
        Array.Empty<RouteEntry>(),
        null,
        Array.Empty<RouteConflict>());
}
