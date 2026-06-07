using System.Text.Json;
using NextNet.Components;
using NextNet.Conventions;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.Routing;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Resolves static path parameters for dynamic routes during static site generation.
/// Checks whether a page component implements <see cref="IStaticPathProvider"/>
/// and calls <see cref="IStaticPathProvider.GetStaticPathsAsync"/> to obtain param sets.
/// </summary>
public class StaticParamsResolver
{
    private readonly IRouteComponentResolver _componentResolver;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISharpFileSystem _fileSystem;
    private readonly INextNetLogger? _logger;

    // Cache resolved param sets keyed by RouteEntry for incremental builds
    private readonly Dictionary<string, IReadOnlyList<Dictionary<string, string>>?> _cache = new();

    /// <summary>
    /// Initializes a new instance of <see cref="StaticParamsResolver"/>.
    /// </summary>
    /// <param name="componentResolver">Resolver for page component types from route entries.</param>
    /// <param name="serviceProvider">DI service provider for instantiating page components.</param>
    /// <param name="fileSystem">File system abstraction for convention-based discovery.</param>
    /// <param name="logger">Optional logger for diagnostic output.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public StaticParamsResolver(
        IRouteComponentResolver componentResolver,
        IServiceProvider serviceProvider,
        ISharpFileSystem? fileSystem = null,
        INextNetLogger? logger = null)
    {
        _componentResolver = componentResolver ?? throw new ArgumentNullException(nameof(componentResolver));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
        _logger = logger;
    }

    /// <summary>
    /// Resolves the static path parameter sets for the given route entry.
    /// Returns <c>null</c> if the route has no static params (i.e., should use SSR).
    /// </summary>
    /// <param name="entry">The route entry to resolve params for.</param>
    /// <returns>A list of parameter dictionaries, or <c>null</c> if none found.</returns>
    public async Task<IReadOnlyList<Dictionary<string, string>>?> ResolveAsync(RouteEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        // Check cache first
        if (_cache.TryGetValue(entry.FilePath, out var cached))
            return cached;

        IReadOnlyList<Dictionary<string, string>>? result = null;

        // Strategy 1: Check if the page component implements IStaticPathProvider
        var pageType = _componentResolver.GetPageType(entry);
        if (pageType != null && typeof(IStaticPathProvider).IsAssignableFrom(pageType))
        {
            result = await ResolveFromProviderAsync(pageType);
        }

        // Strategy 2: Convention-based discovery (staticparams.cs alongside page)
        if (result == null || result.Count == 0)
        {
            result = await ResolveFromConventionFileAsync(entry);
        }

        // Cache the result (even null, to avoid re-checking)
        _cache[entry.FilePath] = result;
        return result;
    }

    /// <summary>
    /// Clears the internal cache. Useful for incremental builds or testing.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Attempts to resolve params by instantiating the page type and casting it
    /// to <see cref="IStaticPathProvider"/>.
    /// </summary>
    private async Task<IReadOnlyList<Dictionary<string, string>>?> ResolveFromProviderAsync(Type pageType)
    {
        try
        {
            // Try resolving by the interface first (most common registration pattern)
            var page = _serviceProvider.GetService(typeof(IStaticPathProvider)) as IStaticPathProvider;
            if (page != null)
            {
                return await page.GetStaticPathsAsync();
            }

            // Fall back to resolving by concrete page type
            var concretePage = _serviceProvider.GetService(pageType) as IStaticPathProvider;
            if (concretePage != null)
            {
                return await concretePage.GetStaticPathsAsync();
            }
        }
        catch (Exception ex)
        {
            // If DI resolution fails, fall through to convention-based approach
            _logger?.Debug("Failed to resolve static params from page type {Type}: {Message}",
                pageType.FullName, ex.Message);
        }

        return null;
    }

    /// <summary>
    /// Attempts to resolve params via convention: look for a <c>staticparams.json</c> file
    /// next to the page component's directory and parse the parameter sets from it.
    /// The JSON file should contain an array of parameter objects, e.g.:
    /// <code>
    /// [
    ///   { "slug": "hello-world" },
    ///   { "slug": "getting-started" }
    /// ]
    /// </code>
    /// </summary>
    private async Task<IReadOnlyList<Dictionary<string, string>>?> ResolveFromConventionFileAsync(RouteEntry entry)
    {
        var directory = _fileSystem.GetDirectoryName(entry.FilePath);
        if (directory == null)
            return null;

        var staticParamsPath = _fileSystem.Combine(directory, "staticparams.json");
        if (!_fileSystem.FileExists(staticParamsPath))
            return null;

        try
        {
            var json = await _fileSystem.ReadAllTextAsync(staticParamsPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var paramSets = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(json, options);

            if (paramSets == null || paramSets.Count == 0)
                return null;

            return paramSets;
        }
        catch (JsonException)
        {
            // Invalid JSON — treat as no-params (SSR fallback)
            return null;
        }
        catch (Exception ex)
        {
            // IO error — treat as no-params (SSR fallback)
            _logger?.Warn("Failed to read static params file: {Path}. {Message}",
                staticParamsPath, ex.Message);
            return null;
        }
    }
}
