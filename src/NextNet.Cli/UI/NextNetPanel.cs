using Spectre.Console;
using Spectre.Console.Rendering;

namespace NextNet.Cli.UI;

/// <summary>
/// Wraps Spectre.Console's <see cref="Panel"/> with NextNet branding.
/// Provides rounded borders with a teal header.
/// </summary>
public sealed class NextNetPanel
{
    private readonly string _title;
    private readonly OutputMode _mode;
    private IRenderable? _content;
    private string? _textContent;

    /// <summary>
    /// Creates a new panel with the specified title.
    /// </summary>
    /// <param name="title">The panel header title.</param>
    /// <param name="mode">Output mode (Color or Plain).</param>
    public NextNetPanel(string title, OutputMode mode = OutputMode.Color)
    {
        _title = title;
        _mode = mode;
    }

    /// <summary>Set the panel content from a string.</summary>
    public NextNetPanel SetContent(string content)
    {
        _textContent = content;
        _content = null;
        return this;
    }

    /// <summary>Set the panel content from a renderable object.</summary>
    public NextNetPanel SetContent(IRenderable content)
    {
        _content = content;
        _textContent = null;
        return this;
    }

    /// <summary>Set the panel content from a factory function.</summary>
    public NextNetPanel SetContent(Func<IRenderable> contentFactory)
    {
        _content = contentFactory();
        _textContent = null;
        return this;
    }

    /// <summary>Build the Spectre.Console panel.</summary>
    public Panel Build()
    {
        IRenderable content = _content ?? new Markup(_textContent ?? "");

        var panel = new Panel(content)
            .Border(_mode == OutputMode.Plain ? BoxBorder.Ascii : BoxBorder.Rounded)
            .BorderColor(_mode == OutputMode.Color ? Theme.BorderColor : Color.Default)
            .Padding(new Padding(2, 0, 2, 0));

        panel.Header = new PanelHeader(_title);

        return panel;
    }

    /// <summary>Expose the underlying Spectre.Console panel.</summary>
    public Panel GetSpectrePanel() => Build();

    /// <summary>Render the panel to the console.</summary>
    public void Render(IAnsiConsole console) => console.Write(Build());
}
