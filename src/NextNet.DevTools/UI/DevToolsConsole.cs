namespace NextNet.DevTools.UI;

/// <summary>
/// Provides formatted console output helpers for DevTools.
/// Includes timestamped logs, success/warning/error indicators, headings, and muted text.
/// Supports optional <see cref="TerminalColorPalette"/> for theme-aware colors.
/// </summary>
/// <remarks>
/// Create an instance with a <see cref="TerminalColorPalette"/> to use theme-aware
/// colors, or use the static convenience methods which use a default (no-palette)
/// instance. The static <see cref="Default"/> property can be set to a custom
/// instance to change behavior globally.
/// </remarks>
/// <example>
/// <code>
/// // Instance with theme-aware palette
/// var palette = new TerminalColorPalette(isDark: true);
/// var console = new DevToolsConsole(palette);
/// console.WriteLog("Server started");
/// console.WriteSuccess("Build completed");
///
/// // Static convenience (backward compatible)
/// DevToolsConsole.WriteLog("Server started");
/// DevToolsConsole.Default = new DevToolsConsole(palette);
/// </code>
/// </example>
public class DevToolsConsole
{
    private readonly TerminalColorPalette? _palette;

    /// <summary>
    /// Gets or sets the default instance used by the static convenience methods.
    /// Initially set to a parameterless instance (no theme palette).
    /// </summary>
    public static DevToolsConsole Default { get; set; } = new DevToolsConsole();

    /// <summary>
    /// Creates a new DevToolsConsole with theme-aware coloring.
    /// </summary>
    /// <param name="palette">
    /// An optional <see cref="TerminalColorPalette"/> for theme-aware console colors.
    /// If null, traditional hardcoded <see cref="ConsoleColor"/> values are used.
    /// </param>
    public DevToolsConsole(TerminalColorPalette? palette = null)
    {
        _palette = palette;
    }

    // ── Instance methods ────────────────────────────────────────────

    /// <summary>Write a line to stdout with a timestamp prefix.</summary>
    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        System.Console.WriteLine($"[{timestamp}] {message}");
    }

    /// <summary>Write a success message with green/theme color and checkmark.</summary>
    public void Success(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Success) ?? ConsoleColor.Green;
        System.Console.WriteLine($"✓ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write a warning message with yellow/theme color and warning symbol.</summary>
    public void Warning(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Warning) ?? ConsoleColor.Yellow;
        System.Console.WriteLine($"⚠ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write an error message with red/theme color and cross mark.</summary>
    public void Error(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Danger) ?? ConsoleColor.Red;
        System.Console.WriteLine($"✗ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write an info message with cyan/theme color and info symbol.</summary>
    public void Info(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Info) ?? ConsoleColor.Cyan;
        System.Console.WriteLine($"ℹ {message}");
        System.Console.ResetColor();
    }

    /// <summary>Write a heading with an underline in the primary theme color.</summary>
    public void Heading(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Primary) ?? ConsoleColor.Cyan;
        System.Console.WriteLine(message);
        System.Console.WriteLine(new string('=', message.Length));
        System.Console.ResetColor();
    }

    /// <summary>Write a muted/subtle line in dark gray/theme color.</summary>
    public void Muted(string message)
    {
        System.Console.ForegroundColor = _palette?.Resolve(DevToolsColorRole.Muted) ?? ConsoleColor.DarkGray;
        System.Console.WriteLine(message);
        System.Console.ResetColor();
    }

    // ── Static convenience methods (delegate to Default) ────────────

    /// <summary>Write a line to stdout with a timestamp prefix.</summary>
    public static void WriteLog(string message) => Default.Log(message);

    /// <summary>Write a success message with green/theme color and checkmark.</summary>
    public static void WriteSuccess(string message) => Default.Success(message);

    /// <summary>Write a warning message with yellow/theme color and warning symbol.</summary>
    public static void WriteWarning(string message) => Default.Warning(message);

    /// <summary>Write an error message with red/theme color and cross mark.</summary>
    public static void WriteError(string message) => Default.Error(message);

    /// <summary>Write an info message with cyan/theme color and info symbol.</summary>
    public static void WriteInfo(string message) => Default.Info(message);

    /// <summary>Write a heading with an underline in the primary theme color.</summary>
    public static void WriteHeading(string message) => Default.Heading(message);

    /// <summary>Write a muted/subtle line in dark gray/theme color.</summary>
    public static void WriteMuted(string message) => Default.Muted(message);
}
