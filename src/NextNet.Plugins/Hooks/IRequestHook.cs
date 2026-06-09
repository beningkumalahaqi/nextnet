using Microsoft.AspNetCore.Http;

namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires for each incoming HTTP request.
/// Plugins can inspect, log, or modify request handling behaviour.
/// </summary>
/// <example>
/// <code>
/// public class MyRequestLogger : NextNetPlugin, IRequestHook
/// {
///     public override string Name => "RequestLogger";
///
///     public Task OnRequestAsync(PluginContext ctx, HttpContext httpContext)
///     {
///         ctx.Logger.Info("Request: {0} {1}",
///             httpContext.Request.Method,
///             httpContext.Request.Path);
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IRequestHook
{
    /// <summary>
    /// Called asynchronously for each incoming HTTP request.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="httpContext">The ASP.NET Core HTTP context for the current request.</param>
    Task OnRequestAsync(PluginContext ctx, HttpContext httpContext);
}
