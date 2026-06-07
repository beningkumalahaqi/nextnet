namespace NextNet.Routing.Models;

/// <summary>
/// Defines the type of a route entry discovered during file-system scanning.
/// </summary>
public enum RouteType
{
    /// <summary>
    /// A page component (e.g. <c>app/about/page.cs</c>).
    /// </summary>
    Page,

    /// <summary>
    /// A layout component (e.g. <c>app/layout.cs</c> or <c>app/blog/layout.cs</c>).
    /// </summary>
    Layout,

    /// <summary>
    /// An API route handler (e.g. <c>app/api/users/route.cs</c>).
    /// </summary>
    Api,

    /// <summary>
    /// An error boundary page (e.g. <c>app/error.cs</c>).
    /// </summary>
    Error,
}
