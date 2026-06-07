using Spectre.Console;
using Spectre.Console.Rendering;

namespace NextNet.Cli.UI;

/// <summary>
/// Central console facade for the NextNet CLI.
/// All formatted output flows through this class. Detects color support and
/// respects <c>NO_COLOR</c>, <c>--plain</c>, and <c>--no-color</c> settings.
/// </summary>
public sealed class NextNetConsole
{
    private readonly IAnsiConsole _console;
    private readonly OutputMode _mode;

    /// <summary>
    /// Initializes a new instance of <see cref="NextNetConsole"/>.
    /// </summary>
    /// <param name="mode">The output mode to use. Auto-detected from environment if not specified.</param>
    public NextNetConsole(OutputMode mode)
    {
        _mode = mode;
        _console = mode == OutputMode.Plain || mode == OutputMode.Json
            ? AnsiConsole.Create(new AnsiConsoleSettings
            {
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(System.Console.Out)
            })
            : AnsiConsole.Console;
    }

    /// <summary>
    /// Creates a console with automatic output mode detection.
    /// </summary>
    /// <param name="plain">Force plain mode.</param>
    /// <param name="noColor">Force no-color mode.</param>
    /// <returns>A configured <see cref="NextNetConsole"/>.</returns>
    public static NextNetConsole Create(bool plain = false, bool noColor = false)
    {
        var mode = DetectMode(plain, noColor);
        return new NextNetConsole(mode);
    }

    /// <summary>
    /// Detects the appropriate output mode based on environment and flags.
    /// </summary>
    public static OutputMode DetectMode(bool plain = false, bool noColor = false)
    {
        if (plain) return OutputMode.Plain;
        if (noColor) return OutputMode.Plain;
        if (Environment.GetEnvironmentVariable("NO_COLOR") is not null) return OutputMode.Plain;
        if (string.Equals(Environment.GetEnvironmentVariable("TERM"), "dumb", StringComparison.OrdinalIgnoreCase))
            return OutputMode.Plain;
        return OutputMode.Color;
    }

    /// <summary>
    /// Gets the current output mode.
    /// </summary>
    public OutputMode Mode => _mode;

    /// <summary>
    /// Whether the console is in plain text mode.
    /// </summary>
    public bool IsPlain => _mode == OutputMode.Plain;

    /// <summary>
    /// Whether the console is in color mode.
    /// </summary>
    public bool IsColor => _mode == OutputMode.Color;

    /// <summary>
    /// Whether the console is in JSON mode.
    /// </summary>
    public bool IsJson => _mode == OutputMode.Json;

    /// <summary>
    /// Gets the underlying Spectre.Console instance.
    /// </summary>
    public IAnsiConsole SpectreConsole => _console;

    // ── Core write methods ───────────────────────────────────────────

    /// <summary>Write raw text to the console.</summary>
    public void Write(string text) => _console.Write(new Markup(text.EscapeMarkup()));

    /// <summary>Write a line of text to the console.</summary>
    public void WriteLine(string text = "") => _console.WriteLine(text);

    /// <summary>Write a heading in bold teal.</summary>
    public void WriteHeading(string text)
    {
        _console.Write(new Markup(text, Theme.HeadingStyle));
        _console.WriteLine();
    }

    /// <summary>Write a sub-heading in bold default color.</summary>
    public void WriteSubheading(string text)
    {
        _console.Write(new Markup(text, new Style(decoration: Decoration.Bold)));
        _console.WriteLine();
    }

    /// <summary>Write muted/dim text.</summary>
    public void WriteMuted(string text)
    {
        _console.Write(new Markup(text, Theme.MutedStyle));
        _console.WriteLine();
    }

    /// <summary>Write a success message with ✓ prefix.</summary>
    public void WriteSuccess(string text)
    {
        var prefix = IsPlain ? "[OK]" : "✓";
        _console.MarkupLine($"[bold {Theme.SuccessHex}]{EscapeMarkup(prefix)} {EscapeMarkup(text)}[/]");
    }

    /// <summary>Write a warning message with ⚠ prefix.</summary>
    public void WriteWarning(string text)
    {
        var prefix = IsPlain ? "[WARN]" : "⚠";
        _console.MarkupLine($"[bold {Theme.WarningHex}]{EscapeMarkup(prefix)} {EscapeMarkup(text)}[/]");
    }

    /// <summary>Write an error message with ✗ prefix.</summary>
    public void WriteError(string text)
    {
        var prefix = IsPlain ? "[ERR]" : "✗";
        _console.MarkupLine($"[bold {Theme.ErrorHex}]{EscapeMarkup(prefix)} {EscapeMarkup(text)}[/]");
    }

    /// <summary>Write an info message with ℹ prefix.</summary>
    public void WriteInfo(string text)
    {
        var prefix = IsPlain ? "[INFO]" : "ℹ";
        _console.MarkupLine($"[bold {Theme.InfoHex}]{EscapeMarkup(prefix)} {EscapeMarkup(text)}[/]");
    }

    /// <summary>Write code-styled text in violet.</summary>
    public void WriteCode(string text)
    {
        _console.Write(new Markup(text, Theme.CodeStyle));
        _console.WriteLine();
    }

    /// <summary>Write a link with violet underline.</summary>
    public void WriteLink(string label, string url)
    {
        if (IsPlain)
            _console.MarkupLine($"{EscapeMarkup(label)}: {EscapeMarkup(url)}");
        else
            _console.MarkupLine($"[link={EscapeMarkup(url)}][underline {Theme.VioletHex}]{EscapeMarkup(label)}[/][/]");
    }

    /// <summary>Write a renderable object to the console.</summary>
    public void Write(IRenderable renderable) => _console.Write(renderable);

    // ── Factory methods ──────────────────────────────────────────────

    /// <summary>Creates a new panel with the specified title.</summary>
    public NextNetPanel CreatePanel(string title) => new(title, _mode);

    /// <summary>Creates a new table with the specified headers.</summary>
    public NextNetTable CreateTable(params string[] headers) => new(headers, _mode);

    /// <summary>Creates a new tree with the specified root label.</summary>
    public NextNetTree CreateTree(string rootLabel) => new(rootLabel, _mode);

    /// <summary>Creates a new multi-step progress display.</summary>
    public NextNetProgress CreateProgress() => new(_mode);

    /// <summary>Creates a new spinner with the specified message.</summary>
    public NextNetSpinner CreateSpinner(string message) => new(message, _mode);

    private static string EscapeMarkup(string text) => text.EscapeMarkup();
}

/// <summary>
/// Simple spinner component that displays an animated spinner with a message.
/// </summary>
public sealed class NextNetSpinner : IDisposable
{
    private readonly string _message;
    private readonly OutputMode _mode;

    internal NextNetSpinner(string message, OutputMode mode)
    {
        _message = message;
        _mode = mode;
    }

    /// <summary>Starts the spinner and runs the action.</summary>
    public async Task StartAsync(Func<Task> action)
    {
        if (_mode == OutputMode.Color)
        {
            var status = new Status(AnsiConsole.Console);
            await status.StartAsync(_message, async _ => await action());
        }
        else
        {
            AnsiConsole.Console.WriteLine($"  {_message}...");
            await action();
        }
    }

    /// <summary>Stops the spinner.</summary>
    public void Dispose() { }
}
