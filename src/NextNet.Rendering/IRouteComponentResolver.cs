using NextNet.Routing;

namespace NextNet.Rendering;

/// <summary>
/// Resolves CLR types for page and layout components from route entries.
/// Used by the SSR engine to instantiate components via DI.
/// </summary>
public interface IRouteComponentResolver
{
    /// <summary>
    /// Gets the CLR type for the page component of the given route entry.
    /// </summary>
    /// <param name="entry">The route entry to resolve a page type for.</param>
    /// <returns>The page component type, or <c>null</c> if not found.</returns>
    Type? GetPageType(RouteEntry entry);

    /// <summary>
    /// Gets the CLR type for the layout component at the given file path.
    /// </summary>
    /// <param name="layoutPath">The file path of the layout (e.g. <c>app/layout.cs</c>).</param>
    /// <returns>The layout component type, or <c>null</c> if not found.</returns>
    Type? GetLayoutType(string layoutPath);
}
