namespace NextNet.Components;

/// <summary>
/// Provides access to route parameters extracted from the matched URL pattern.
/// </summary>
/// <remarks>
/// An instance of <see cref="Route"/> is available via the <see cref="Page.Route"/> property
/// on any page that inherits from the <see cref="Page"/> base class.
/// </remarks>
/// <example>
/// <code>
/// public class BlogPostPage : Page
/// {
///     public override async Task&lt;IHtmlContent&gt; Render()
///     {
///         var slug = Route.Params["slug"];
///         return HtmlHelper.Text(slug);
///     }
/// }
/// </code>
/// </example>
public sealed class Route
{
    /// <summary>
    /// Gets the read-only dictionary of route parameters.
    /// </summary>
    /// <remarks>
    /// Parameters are keyed by the parameter name (case-insensitive).
    /// For a route pattern <c>/blog/{slug}</c> and URL <c>/blog/hello-world</c>,
    /// <c>Params["slug"]</c> returns <c>"hello-world"</c>.
    /// </remarks>
    public IReadOnlyDictionary<string, string> Params { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Route"/> class.
    /// </summary>
    /// <param name="routeParams">The route parameters dictionary from the component context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="routeParams"/> is <c>null</c>.</exception>
    public Route(IReadOnlyDictionary<string, string> routeParams)
    {
        Params = routeParams ?? throw new ArgumentNullException(nameof(routeParams));
    }
}
