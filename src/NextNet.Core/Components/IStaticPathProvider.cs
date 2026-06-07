namespace NextNet.Components;

/// <summary>
/// Provides static path parameters for dynamic routes with parameterised segments.
/// Implemented by page components to specify which concrete paths should be
/// pre-rendered at build time during Static Site Generation (SSG).
/// </summary>
/// <example>
/// For a route <c>/blog/[slug]</c>, implement <see cref="IStaticPathProvider"/>
/// on the page class and return param sets like <c>["slug" = "hello-world"]</c>.
/// </example>
public interface IStaticPathProvider
{
    /// <summary>
    /// Returns a list of parameter dictionaries that define the concrete paths
    /// to pre-render. Each dictionary maps parameter names to their values.
    /// </summary>
    /// <returns>
    /// A task that resolves to a list of parameter dictionaries.
    /// An empty list means no paths will be pre-rendered (the route will use SSR).
    /// </returns>
    Task<IReadOnlyList<Dictionary<string, string>>> GetStaticPathsAsync();
}
