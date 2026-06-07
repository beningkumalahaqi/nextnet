using NextNet.Isr.Cache;

namespace NextNet.Isr.Manifest;

/// <summary>
/// An immutable snapshot of all ISR-configured routes and their metadata.
/// Generated at build/startup time and consumed by the ISR middleware
/// to determine revalidation behaviour per route.
/// </summary>
public class IsrManifest
{
    /// <summary>
    /// Gets the dictionary of route patterns to their ISR metadata.
    /// Keys are route patterns (e.g. <c>"/blog/{slug}"</c>).
    /// </summary>
    public IReadOnlyDictionary<string, IsrRouteMetadata> Routes { get; }

    /// <summary>
    /// Gets the global ISR options that serve as defaults for routes without explicit configuration.
    /// </summary>
    public IsrGlobalOptions GlobalOptions { get; }

    /// <summary>
    /// Gets a value indicating whether any ISR routes are configured.
    /// </summary>
    public bool HasIsrRoutes => Routes.Count > 0;

    /// <summary>
    /// Initializes a new instance of <see cref="IsrManifest"/>.
    /// </summary>
    /// <param name="routes">The dictionary of route-to-metadata mappings.</param>
    /// <param name="globalOptions">The global ISR options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IsrManifest(
        IReadOnlyDictionary<string, IsrRouteMetadata> routes,
        IsrGlobalOptions globalOptions)
    {
        Routes = routes ?? throw new ArgumentNullException(nameof(routes));
        GlobalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// Attempts to get the ISR metadata for the specified route.
    /// Falls back to the global defaults if no per-route metadata exists.
    /// </summary>
    /// <param name="route">The route path (e.g. <c>"/blog/hello-world"</c>).</param>
    /// <param name="metadata">When this method returns, contains the route metadata if found.</param>
    /// <returns><c>true</c> if metadata was found for the route; otherwise <c>false</c>.</returns>
    public bool TryGetMetadata(string route, out IsrRouteMetadata? metadata)
    {
        // Try exact match first
        if (Routes.TryGetValue(route, out metadata))
            return true;

        // Try matching against route patterns (e.g. "/blog/{slug}" matches "/blog/hello")
        foreach (var kvp in Routes)
        {
            if (MatchesPattern(kvp.Key, route))
            {
                metadata = kvp.Value;
                return true;
            }
        }

        metadata = null;
        return false;
    }

    /// <summary>
    /// Gets the <see cref="IsrRouteMetadata"/> for the specified route, or creates
    /// a default from global options if no per-route metadata exists.
    /// </summary>
    public IsrRouteMetadata GetMetadataOrDefault(string route)
    {
        if (TryGetMetadata(route, out var metadata) && metadata != null)
            return metadata;

        return new IsrRouteMetadata
        {
            RoutePattern = route,
            RevalidateSeconds = GlobalOptions.DefaultRevalidateSeconds,
            MaxConcurrentRegenerations = 1,
            ServeStaleWhileRevalidate = true
        };
    }

    private static bool MatchesPattern(string pattern, string route)
    {
        var patternSegments = pattern.Trim('/').Split('/');
        var routeSegments = route.Trim('/').Split('/');

        if (patternSegments.Length != routeSegments.Length)
            return false;

        for (int i = 0; i < patternSegments.Length; i++)
        {
            var ps = patternSegments[i];
            if (ps.StartsWith('{') && ps.EndsWith('}'))
                continue; // Dynamic segment matches anything
            if (!string.Equals(ps, routeSegments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns an empty manifest with no ISR routes.
    /// </summary>
    public static IsrManifest Empty { get; } = new(
        new Dictionary<string, IsrRouteMetadata>(),
        new IsrGlobalOptions());
}
