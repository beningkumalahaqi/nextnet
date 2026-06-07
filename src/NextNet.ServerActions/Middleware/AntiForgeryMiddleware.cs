using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace NextNet.ServerActions.Middleware;

/// <summary>
/// ASP.NET Core middleware that validates anti-forgery tokens on all
/// POST requests to <c>/_actions/</c>. Relies on the registered
/// <see cref="IAntiforgery"/> service (added via
/// <c>services.AddAntiforgery()</c> in the host application).
/// </summary>
public sealed class AntiForgeryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAntiforgery _antiforgery;

    /// <summary>
    /// Initializes a new instance of <see cref="AntiForgeryMiddleware"/>.
    /// </summary>
    public AntiForgeryMiddleware(RequestDelegate next, IAntiforgery antiforgery)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _antiforgery = antiforgery ?? throw new ArgumentNullException(nameof(antiforgery));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var path = context.Request.Path.Value ?? string.Empty;

        // Only validate on POST requests to /_actions/
        if (HttpMethods.IsPost(context.Request.Method) &&
            path.StartsWith("/_actions/", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                await _antiforgery.ValidateRequestAsync(context);
            }
            catch (AntiforgeryValidationException)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "Anti-forgery token validation failed.",
                    isSuccess = false,
                    isError = true
                });
                return;
            }
        }

        await _next(context);
    }
}
