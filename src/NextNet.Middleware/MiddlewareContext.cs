using Microsoft.AspNetCore.Http;

namespace NextNet.Middleware;

/// <summary>
/// Provides context for middleware execution, including access to the
/// HTTP context, a per-request items dictionary, and the owning pipeline.
/// </summary>
public class MiddlewareContext
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
    public MiddlewareContext(HttpContext httpContext, MiddlewarePipeline pipeline)
    {
        HttpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
        Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Items = new Dictionary<string, object?>();
    }
}
