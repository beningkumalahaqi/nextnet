using Microsoft.AspNetCore.Http;
using NextNet.Isr.Endpoints;
using NextNet.Logging;

namespace NextNet.Isr.Middleware;

/// <summary>
/// ASP.NET Core middleware that intercepts requests to <c>/_isr/revalidate</c>
/// and delegates them to the <see cref="IsrRevalidationEndpoint"/> handler.
/// </summary>
public class IsrRevalidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IsrRevalidationEndpoint _endpoint;
    private readonly INextNetLogger? _logger;

    private const string EndpointPath = "/_isr/revalidate";

    /// <summary>
    /// Initializes a new instance of <see cref="IsrRevalidationMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="endpoint">The revalidation endpoint handler.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when required dependencies are null.</exception>
    public IsrRevalidationMiddleware(
        RequestDelegate next,
        IsrRevalidationEndpoint endpoint,
        INextNetLogger? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware for the given HTTP context.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;

        if (string.Equals(path, EndpointPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger?.Debug("Handling ISR revalidation request");
            await _endpoint.HandleAsync(context);
            return;
        }

        await _next(context);
    }
}
