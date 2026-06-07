namespace NextNet.Plugins;

/// <summary>
/// Convenience base class for NextNet plugins.
/// Override only the members you need; all provide default no-op implementations.
/// </summary>
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
