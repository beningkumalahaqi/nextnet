using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;

namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires during the rendering pipeline — before and after rendering.
/// Plugins can inject content, modify rendered output, or add tracking.
/// </summary>
/// <example>
/// <code>
/// public class MyAnalyticsPlugin : NextNetPlugin, IRenderHook
/// {
///     public override string Name => "Analytics";
///
///     public Task OnPreRender(PluginContext ctx, HttpContext httpContext)
///     {
///         ctx.Logger.Info("Rendering page: {0}", httpContext.Request.Path);
///         return Task.CompletedTask;
///     }
///
///     public Task OnPostRender(PluginContext ctx, IHtmlContent content)
///     {
///         // Wrap or inspect the rendered HTML content
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IRenderHook
{
    /// <summary>
    /// Called before the page/component is rendered.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="httpContext">The ASP.NET Core HTTP context for the current request.</param>
    Task OnPreRender(PluginContext ctx, HttpContext httpContext);

    /// <summary>
    /// Called after the page/component has been rendered to HTML.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="content">The rendered HTML content. Can be modified by wrapping or replacing the content.</param>
    Task OnPostRender(PluginContext ctx, IHtmlContent content);
}
