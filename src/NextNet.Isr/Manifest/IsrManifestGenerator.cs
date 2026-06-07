using NextNet.Routing;

namespace NextNet.Isr.Manifest;

/// <summary>
/// Generates an <see cref="IsrManifest"/> from the route manifest and
/// per-route ISR configuration attributes/static properties.
/// Scans all page routes and collects their ISR metadata.
/// </summary>
public class IsrManifestGenerator
{
    private readonly RouteManifest _routeManifest;
    private readonly IsrGlobalOptions _globalOptions;

    /// <summary>
    /// Initializes a new instance of <see cref="IsrManifestGenerator"/>.
    /// </summary>
    /// <param name="routeManifest">The route manifest from the routing system.</param>
    /// <param name="globalOptions">The global ISR configuration.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public IsrManifestGenerator(
        RouteManifest routeManifest,
        IsrGlobalOptions globalOptions)
    {
        _routeManifest = routeManifest ?? throw new ArgumentNullException(nameof(routeManifest));
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
    }

    /// <summary>
    /// Generates the ISR manifest by collecting metadata from all page routes.
    /// Routes without explicit ISR configuration inherit global defaults.
    /// </summary>
    /// <returns>An <see cref="IsrManifest"/> containing ISR metadata for all configured routes.</returns>
    public IsrManifest Generate()
    {
        var routes = new Dictionary<string, IsrRouteMetadata>(StringComparer.OrdinalIgnoreCase);

        foreach (var page in _routeManifest.Pages)
        {
            var metadata = CreateMetadata(page);
            routes[page.RoutePattern] = metadata;
        }

        return new IsrManifest(routes, _globalOptions);
    }

    /// <summary>
    /// Creates <see cref="IsrRouteMetadata"/> for a single route entry.
    /// Attempts to discover ISR configuration from:
    /// 1. An <c>[IsrRoute]</c> attribute on the page type
    /// 2. A static <c>IsrConfig</c> property on the page type
    /// 3. Falls back to global defaults
    /// </summary>
    /// <param name="entry">The route entry.</param>
    /// <returns>Route metadata with defaults applied.</returns>
    protected internal IsrRouteMetadata CreateMetadata(RouteEntry entry)
    {
        // Try to load ISR configuration attributes from the page type
        var pageType = TryLoadPageType(entry);

        if (pageType != null)
        {
            // Check for IsrRouteAttribute
            var attribute = pageType.GetCustomAttributes(typeof(IsrRouteAttribute), false)
                .Cast<IsrRouteAttribute>()
                .FirstOrDefault();

            if (attribute != null)
            {
                return new IsrRouteMetadata
                {
                    RoutePattern = entry.RoutePattern,
                    RevalidateSeconds = attribute.RevalidateSeconds,
                    Tags = attribute.Tags,
                    MaxConcurrentRegenerations = attribute.MaxConcurrentRegenerations,
                    ServeStaleWhileRevalidate = attribute.ServeStaleWhileRevalidate,
                    FilePath = entry.FilePath
                };
            }
        }

        // No explicit ISR config — use global defaults
        return new IsrRouteMetadata
        {
            RoutePattern = entry.RoutePattern,
            RevalidateSeconds = _globalOptions.DefaultRevalidateSeconds,
            Tags = Array.Empty<string>(),
            MaxConcurrentRegenerations = 1,
            ServeStaleWhileRevalidate = true,
            FilePath = entry.FilePath
        };
    }

    /// <summary>
    /// Attempts to load the compiled page type for a route entry.
    /// Returns <c>null</c> if the type cannot be resolved.
    /// </summary>
    protected internal static Type? TryLoadPageType(RouteEntry entry)
    {
        try
        {
            // Attempt to find the type by assembly-qualified name or file path convention
            if (!string.IsNullOrEmpty(entry.FilePath))
            {
                // Convert file path to type name (e.g., "app/Pages/Blog/Index.cs" -> "App.Pages.Blog.Index")
                var typeName = ConvertFilePathToTypeName(entry.FilePath);
                var assembly = System.Reflection.Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    return assembly.GetType(typeName, throwOnError: false, ignoreCase: true);
                }
            }
        }
        catch
        {
            // Silently fall through — type resolution is best-effort
        }

        return null;
    }

    /// <summary>
    /// Converts a relative file path to a CLR type name.
    /// E.g., "app/Pages/Blog/Index.cs" -> "App.Pages.Blog.Index"
    /// </summary>
    internal static string ConvertFilePathToTypeName(string filePath)
    {
        var sanitized = filePath
            .Replace('/', '.')
            .Replace('\\', '.')
            .TrimStart('.');

        // Remove file extension
        var lastDot = sanitized.LastIndexOf('.');
        if (lastDot > 0)
        {
            sanitized = sanitized[..lastDot];
        }

        // Capitalize first letter of each segment if needed
        var segments = sanitized.Split('.');
        for (int i = 0; i < segments.Length; i++)
        {
            if (segments[i].Length > 0 && char.IsLower(segments[i][0]))
            {
                segments[i] = char.ToUpperInvariant(segments[i][0]) + segments[i][1..];
            }
        }

        return string.Join(".", segments);
    }
}
