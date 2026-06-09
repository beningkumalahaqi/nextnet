using Microsoft.AspNetCore.Http;

namespace NextNet.Middleware;

/// <summary>
/// Provides context for middleware execution, including access to the
/// HTTP context, a per-request items dictionary, and the owning pipeline.
/// </summary>
/// <example>
/// <code>
/// // MiddlewareContext is created by the pipeline for each request
/// public class MyMiddleware : IMiddleware
/// {
///     public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
///     {
///         // Access HTTP context
///         var path = context.HttpContext.Request.Path;
///
///         // Store data for downstream middleware
///         context.Items["my-data"] = "value";
///
///         await next(context.HttpContext);
///     }
/// }
/// </code>
/// </example>
public sealed record MiddlewareContext
{
    /// <summary>
    /// Gets the current <see cref="HttpContext"/> for the request.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Gets a per-request dictionary for storing and sharing data between middleware.
    /// Items are scoped to the current request and disposed after the request completes.
    /// </summary>
    public IDictionary<string, object?> Items { get; }

    /// <summary>
    /// Gets the <see cref="MiddlewarePipeline"/> that owns this context.
    /// </summary>
    public MiddlewarePipeline Pipeline { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MiddlewareContext"/> class.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="pipeline">The owning middleware pipeline.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="httpContext"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pipeline"/> is null.</exception>
    public MiddlewareContext(HttpContext httpContext, MiddlewarePipeline pipeline)
    {
        HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Items = new Dictionary<string, object?>();
    }
}
