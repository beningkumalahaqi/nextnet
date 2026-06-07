using NextNet.Routing.Models;

namespace NextNet.Routing;

/// <summary>
/// An immutable snapshot of the discovered routes, pages, layouts, API routes,
/// error pages, and any conflicts found during scanning.
/// </summary>
public class RouteManifest
{
    /// <summary>
    /// Gets all discovered route entries across all types.
    /// </summary>
    public IReadOnlyList<RouteEntry> Routes { get; }

    /// <summary>
    /// Gets the discovered page entries (<see cref="RouteType.Page"/>).
    /// </summary>
    public IReadOnlyList<RouteEntry> Pages { get; }

    /// <summary>
    /// Gets the discovered layout entries (<see cref="RouteType.Layout"/>).
    /// </summary>
    public IReadOnlyList<RouteEntry> Layouts { get; }

    /// <summary>
    /// Gets the discovered API route entries (<see cref="RouteType.Api"/>).
    /// </summary>
    public IReadOnlyList<RouteEntry> ApiRoutes { get; }

    /// <summary>
    /// Gets the error page entry, if one was found (<see cref="RouteType.Error"/>).
    /// </summary>
    public RouteEntry? ErrorPage { get; }

    /// <summary>
    /// Gets the list of conflicts discovered between routes.
    /// </summary>
    public IReadOnlyList<RouteConflict> Conflicts { get; }

    /// <summary>
    /// Gets a value indicating whether the manifest contains any conflicts.
    /// </summary>
    public bool HasConflicts => Conflicts.Count > 0;

    /// <summary>
    /// Initializes a new instance of <see cref="RouteManifest"/>.
    /// </summary>
    public RouteManifest(
        IReadOnlyList<RouteEntry> routes,
        IReadOnlyList<RouteEntry> pages,
        IReadOnlyList<RouteEntry> layouts,
        IReadOnlyList<RouteEntry> apiRoutes,
        RouteEntry? errorPage,
        IReadOnlyList<RouteConflict> conflicts)
    {
        Routes = routes;
        Pages = pages;
        Layouts = layouts;
        ApiRoutes = apiRoutes;
        ErrorPage = errorPage;
        Conflicts = conflicts;
    }

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
