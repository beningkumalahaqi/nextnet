namespace NextNet.Components;

/// <summary>
/// Base class for NextNet page components that provides access to route parameters
/// and the component context.
/// </summary>
/// <remarks>
/// Pages inherit from this class and override the <see cref="Render"/> method.
/// Route parameters are accessible via the <see cref="Route"/> property using the
/// <c>Route.Params["name"]</c> indexer pattern.
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
public abstract class Page : IPage, IComponentContextAware
{
    /// <summary>
    /// Gets the route information for the current page, including route parameters.
    /// </summary>
    /// <remarks>
    /// This property is set automatically by the rendering pipeline before <see cref="Render"/> is called.
    /// Access route parameters via <c>Route.Params["paramName"]</c>.
    /// </remarks>
    public Route Route { get; private set; } = default!;

    /// <summary>
    /// Gets the component context for the current request.
    /// </summary>
    /// <remarks>
    /// The context provides access to:
    /// <list type="bullet">
    ///   <item><description>The underlying ASP.NET Core <see cref="Microsoft.AspNetCore.Http.HttpContext"/></description></item>
    ///   <item><description>Route parameters from the matched URL pattern</description></item>
    ///   <item><description>Query string parameters from the URL</description></item>
    /// </list>
    /// <c>Note:</c> This property is <c>protected</c>. Access it from derived page classes only.
    /// </remarks>
    protected ComponentContext Context { get; private set; } = default!;

    /// <inheritdoc />
    IReadOnlyDictionary<string, object> IPage.Props { get; } = new Dictionary<string, object>();

    /// <inheritdoc />
    void IComponentContextAware.SetContext(ComponentContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        Context = context;
        Route = new Route(context.RouteParams);
    }

    /// <summary>
    /// Renders the page content to HTML.
    /// </summary>
    /// <returns>A task representing the asynchronous render operation, with the HTML content.</returns>
    /// <remarks>
    /// Override this method to implement the page's rendering logic.
    /// Use <see cref="HtmlHelper"/> static methods to build HTML content,
    /// and access route parameters via <c>Route.Params["name"]</c>.
    /// </remarks>
    public abstract Task<IHtmlContent> Render();
}
