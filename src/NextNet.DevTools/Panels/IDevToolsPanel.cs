namespace NextNet.DevTools;

/// <summary>
/// Represents a single DevTools panel that can be rendered in the TUI.
/// Panels are registered in <see cref="DevToolsServer"/> and switched via keyboard shortcuts.
/// </summary>
/// <example>
/// <code>
/// public sealed class MyPanel : IDevToolsPanel
/// {
///     public string Name => "My Panel";
///     public string Icon => "🔧";
///
///     public void Render(TuiRenderContext context)
///     {
///         Console.WriteLine("Hello from MyPanel!");
///     }
///
///     public void HandleInput(ConsoleKey key)
///     {
///         if (key == ConsoleKey.Space)
///             Console.WriteLine("Space pressed!");
///     }
/// }
/// </code>
/// </example>
public interface IDevToolsPanel
{
    /// <summary>Display name of the panel shown in the tab bar.</summary>
    string Name { get; }

    /// <summary>Unicode icon shown next to the panel name in the tab bar.</summary>
    string Icon { get; }

    /// <summary>Render the panel content to the TUI.</summary>
    void Render(TuiRenderContext context);

    /// <summary>Handle keyboard input for panel-specific interactions.</summary>
    void HandleInput(ConsoleKey key);
}
