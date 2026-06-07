using Microsoft.AspNetCore.Http;

namespace NextNet.Middleware;

/// <summary>
/// Defines the contract for NextNet middleware components.
/// Middleware can intercept and modify HTTP requests and responses
/// within the NextNet middleware pipeline.
/// </summary>
public interface IMiddleware
{
    /// <summary>
    /// Invokes the middleware with the given context and next delegate.
    /// </summary>
    /// <param name="context">The middleware context providing access to the HTTP context, items, and pipeline.</param>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task InvokeAsync(MiddlewareContext context, RequestDelegate next);
}
