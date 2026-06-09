namespace NextNet.Plugins.Hooks;

/// <summary>
/// Hook that fires when an unhandled exception occurs in the NextNet pipeline.
/// Plugins can log errors, send telemetry, or provide custom error responses.
/// </summary>
/// <example>
/// <code>
/// public class MyErrorLogger : NextNetPlugin, IErrorHook
/// {
///     public override string Name => "ErrorLogger";
///
///     public Task OnError(PluginContext ctx, Exception exception)
///     {
///         ctx.Logger.Error("Unhandled exception: {0}", exception.Message);
///         // Send telemetry or return a custom error response
///         return Task.CompletedTask;
///     }
/// }
/// </code>
/// </example>
public interface IErrorHook
{
    /// <summary>
    /// Called when an exception occurs.
    /// </summary>
    /// <param name="ctx">The plugin context.</param>
    /// <param name="exception">The exception that was thrown.</param>
    Task OnError(PluginContext ctx, Exception exception);
}
