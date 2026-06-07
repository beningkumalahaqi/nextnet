using Microsoft.AspNetCore.Http;
using NextNet.ServerActions.ServerActions;

namespace NextNet.ServerActions.Middleware;

/// <summary>
/// ASP.NET Core middleware that routes requests under <c>/_actions/</c> to the
/// appropriate server action handler.
/// </summary>
public sealed class ServerActionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ServerActionExecutor _executor;
    private readonly ServerActionRegistry _registry;

    /// <summary>
    /// Initializes a new instance of <see cref="ServerActionMiddleware"/>.
    /// </summary>
    public ServerActionMiddleware(
        RequestDelegate next,
        ServerActionExecutor executor,
        ServerActionRegistry registry)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));

        var path = context.Request.Path.Value ?? string.Empty;

        // Only handle POST requests to /_actions/{actionName}
        if (!HttpMethods.IsPost(context.Request.Method) ||
            !path.StartsWith("/_actions/", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Extract action name from path
        var actionName = path.Substring("/_actions/".Length).Trim('/');

        if (string.IsNullOrWhiteSpace(actionName))
        {
            context.Response.StatusCode = 400;
            context.Response.ContentType = "application/json; charset=utf-8";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Action name is required.",
                isSuccess = false,
                isError = true
            });
            return;
        }

        // Execute the action
        await _executor.ExecuteAsync(context, actionName, context.RequestAborted);
    }
}
