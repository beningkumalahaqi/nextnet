using Microsoft.AspNetCore.Http;

namespace NextNet.Components;

/// <summary>
/// Provides context for the currently executing component, including
/// access to the underlying <see cref="Microsoft.AspNetCore.Http.HttpContext"/> and parsed
/// route/query parameters.
/// </summary>
/// <example>
/// <code>
/// public class MyPage : IPage
/// {
///     public async Task&lt;IHtmlContent&gt; Render()
///     {
///         var slug = Context.RouteParams["slug"];
///         var page = Context.QueryParams.GetValueOrDefault("page", "1");
///         return HtmlHelper.Text($"Viewing {slug}, page {page}");
///     }
///
///     public ComponentContext Context { get; set; }
///     public IReadOnlyDictionary&lt;string, object&gt; Props { get; }
/// }
/// </code>
/// </example>
public sealed class ComponentContext
{
    /// <summary>
    /// Gets the underlying ASP.NET Core <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Gets a read-only dictionary of route parameters extracted from the URL.
    /// For example, for a route <c>/blog/{slug}</c> and URL <c>/blog/hello-world</c>,
    /// this will contain <c>{"slug", "hello-world"}</c>.
    /// </summary>
    public IReadOnlyDictionary<string, string> RouteParams { get; }

    /// <summary>
    /// Gets a read-only dictionary of query string parameters from the URL.
    /// </summary>
    public IReadOnlyDictionary<string, string> QueryParams { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentContext"/> wrapping the
    /// specified <paramref name="httpContext"/>.
    /// </summary>
    /// <param name="httpContext">The ASP.NET Core HTTP context.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is <c>null</c>.</exception>
    public ComponentContext(HttpContext httpContext)
    {
        HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));

        // Extract query parameters
        var queryParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in httpContext.Request.Query)
        {
            queryParams[key] = value.ToString();
        }
        QueryParams = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(queryParams);

        // Extract route parameters
        var routeParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in httpContext.Request.RouteValues)
        {
            if (value is string str)
            {
                routeParams[key] = str;
            }
        }
        RouteParams = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(routeParams);
    }
}
