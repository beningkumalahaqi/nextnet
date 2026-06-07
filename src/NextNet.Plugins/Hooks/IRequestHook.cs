using Microsoft.AspNetCore.Http;

namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires for each incoming HTTP request.
/// Plugins can inspect, log, or modify request handling behaviour.
/// </summary>
public interface IRequestHook
{
    /// <summary>
    /// Called asynchronously for each incoming HTTP request.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="httpContext">The ASP.NET Core HTTP context for the current request.</param>
    Task OnRequestAsync(PluginContext ctx, HttpContext httpContext);
}
