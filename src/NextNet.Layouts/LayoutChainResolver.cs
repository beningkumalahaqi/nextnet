using System.Collections.Concurrent;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.Routing;

namespace NextNet.Layouts;

/// <summary>
/// Resolves layout chains from route entries by mapping layout file paths
/// (from <see cref="RouteEntry.LayoutChain"/>) to CLR types via
/// <see cref="IRouteComponentResolver"/>.
/// </summary>
public class LayoutChainResolver
{
    private readonly IRouteComponentResolver _componentResolver;
    private readonly INextNetLogger? _logger;
    private readonly ConcurrentDictionary<string, IReadOnlyList<Type>> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Gets the maximum depth allowed for layout chains.
    /// If a chain exceeds this depth, an exception is thrown to prevent
    /// runaway nesting.
    /// </summary>
    public int MaxDepth { get; set; } = 10;

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutChainResolver"/>.
    /// </summary>
    /// <param name="componentResolver">The component resolver used to map file paths to CLR types.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="componentResolver"/> is <c>null</c>.</exception>
    public LayoutChainResolver(IRouteComponentResolver componentResolver, INextNetLogger? logger = null)
    {
        _componentResolver = componentResolver ?? throw new ArgumentNullException(nameof(componentResolver));
        _logger = logger;
    }

    /// <summary>
    /// Resolves the ordered list of layout types for the given route entry.
    /// The result is ordered from innermost (nearest to the page) to outermost (root).
    /// Results are cached per route pattern.
    /// </summary>
    /// <param name="entry">The route entry to resolve the layout chain for.</param>
    /// <returns>An ordered list of layout types, innermost first.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entry"/> is <c>null</c>.</exception>
    /// <exception cref="RenderException">Thrown when a layout type cannot be resolved or the chain exceeds <see cref="MaxDepth"/>.</exception>
    public IReadOnlyList<Type> ResolveChain(RouteEntry entry)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));

        // Check cache first
        if (_cache.TryGetValue(entry.RoutePattern, out var cached))
        {
            return cached;
        }

        var layoutTypes = ResolveChainCore(entry);
        _cache.TryAdd(entry.RoutePattern, layoutTypes);
        return layoutTypes;
    }

    /// <summary>
    /// Clears all cached layout chains.
    /// Useful when routes change at runtime (e.g. in development mode).
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        _logger?.Debug("Layout chain cache cleared");
    }

    /// <summary>
    /// Core resolution that walks the file path chain and maps each to a CLR type.
    /// </summary>
    private IReadOnlyList<Type> ResolveChainCore(RouteEntry entry)
    {
        if (entry.LayoutChain.Count == 0)
        {
            return Array.Empty<Type>();
        }

        if (entry.LayoutChain.Count > MaxDepth)
        {
            throw new RenderException(
                $"Layout chain for route '{entry.RoutePattern}' has {entry.LayoutChain.Count} levels, " +
                $"which exceeds the maximum depth of {MaxDepth}. This may indicate a circular layout reference.");
        }

        var types = new List<Type>(entry.LayoutChain.Count);
        foreach (var layoutPath in entry.LayoutChain)
        {
            var layoutType = _componentResolver.GetLayoutType(layoutPath);
            if (layoutType == null)
            {
                throw new RenderException(
                    $"Cannot resolve layout type for path '{layoutPath}' (route: {entry.RoutePattern}). " +
                    "Ensure the layout file exists and implements ILayout.");
            }

            if (!typeof(ILayout).IsAssignableFrom(layoutType))
            {
                throw new RenderException(
                    $"Type '{layoutType.FullName}' (resolved from '{layoutPath}') does not implement ILayout.");
            }

            _logger?.Debug("Resolved layout {LayoutPath} -> {LayoutType}", layoutPath, layoutType.Name);
            types.Add(layoutType);
        }

        return types.AsReadOnly();
    }
}
