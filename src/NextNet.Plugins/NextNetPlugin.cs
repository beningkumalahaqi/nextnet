namespace NextNet.Plugins;

/// <summary>
/// Convenience base class for NextNet plugins.
/// Override only the members you need; all provide default no-op implementations.
/// </summary>
/// <example>
/// <code>
/// public class MyPlugin : NextNetPlugin
/// {
///     public override string Name => "MyPlugin";
///     public override string Description => "Does something useful";
///     public override Version Version => new(1, 0, 0);
///
///     public override async Task OnInitializeAsync(PluginContext context)
///     {
///         await Task.CompletedTask;
///         context.Logger.Info("MyPlugin initialized.");
///     }
/// }
/// </code>
/// </example>
public abstract class NextNetPlugin : INextNetPlugin
{
    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public virtual string Description => string.Empty;

    /// <inheritdoc />
    public virtual Version Version => new(1, 0, 0);

    /// <inheritdoc />
    public virtual Task OnInitializeAsync(PluginContext context) => Task.CompletedTask;
}
