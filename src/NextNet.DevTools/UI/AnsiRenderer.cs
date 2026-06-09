using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// Provides ANSI escape code helpers for terminal text formatting.
/// Supports 16-color, 256-color, and truecolor (24-bit) foreground/background,
/// as well as style attributes (bold, dim, italic, reset).
/// </summary>
/// <remarks>
/// All methods return ANSI escape sequences as strings. Use <see cref="Reset"/>
/// to terminate a styled segment. Methods can be chained to combine styles:
/// <code>
/// var styled = AnsiRenderer.Bold + AnsiRenderer.Fg(ConsoleColor.Red) + "error" + AnsiRenderer.Reset;
/// </code>
/// </remarks>
/// <example>
/// <code>
/// // 16-color foreground
/// Console.Write(AnsiRenderer.Fg(ConsoleColor.Green) + "OK" + AnsiRenderer.Reset);
///
/// // TrueColor foreground
/// Console.Write(AnsiRenderer.FgRgb(255, 128, 0) + "warm" + AnsiRenderer.Reset);
///
/// // Convenience: combine color and text
/// Console.WriteLine(AnsiRenderer.WithColor("bold text", ConsoleColor.Cyan, bold: true));
/// </code>
/// </example>
public static class AnsiRenderer
{
    /// <summary>ANSI escape prefix.</summary>
    private const string Esc = "\u001b[";

    /// <summary>ANSI style: bold (increased intensity).</summary>
    public static string Bold => Esc + "1m";

    /// <summary>ANSI style: dim (decreased intensity).</summary>
    public static string Dim => Esc + "2m";

    /// <summary>ANSI style: italic.</summary>
    public static string Italic => Esc + "3m";

    /// <summary>ANSI style: underline.</summary>
    public static string Underline => Esc + "4m";

    /// <summary>Resets all ANSI formatting to terminal defaults.</summary>
    public static string Reset => Esc + "0m";

    /// <summary>
    /// Returns an ANSI escape sequence to set the foreground color to the
    /// specified 16-color <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="color">The 16-color console color to use as foreground.</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[31m" for red).</returns>
    public static string Fg(ConsoleColor color)
    {
        var code = color switch
        {
            ConsoleColor.Black => 30,
            ConsoleColor.DarkRed => 31,
            ConsoleColor.DarkGreen => 32,
            ConsoleColor.DarkYellow => 33,
            ConsoleColor.DarkBlue => 34,
            ConsoleColor.DarkMagenta => 35,
            ConsoleColor.DarkCyan => 36,
            ConsoleColor.Gray => 37,
            ConsoleColor.DarkGray => 90,
            ConsoleColor.Red => 91,
            ConsoleColor.Green => 92,
            ConsoleColor.Yellow => 93,
            ConsoleColor.Blue => 94,
            ConsoleColor.Magenta => 95,
            ConsoleColor.Cyan => 96,
            ConsoleColor.White => 97,
            _ => 39 // default foreground
        };
        return Esc + code + "m";
    }

    /// <summary>
    /// Returns an ANSI escape sequence to set the background color to the
    /// specified 16-color <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="color">The 16-color console color to use as background.</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[41m" for red background).</returns>
    public static string Bg(ConsoleColor color)
    {
        var code = color switch
        {
            ConsoleColor.Black => 40,
            ConsoleColor.DarkRed => 41,
            ConsoleColor.DarkGreen => 42,
            ConsoleColor.DarkYellow => 43,
            ConsoleColor.DarkBlue => 44,
            ConsoleColor.DarkMagenta => 45,
            ConsoleColor.DarkCyan => 46,
            ConsoleColor.Gray => 47,
            ConsoleColor.DarkGray => 100,
            ConsoleColor.Red => 101,
            ConsoleColor.Green => 102,
            ConsoleColor.Yellow => 103,
            ConsoleColor.Blue => 104,
            ConsoleColor.Magenta => 105,
            ConsoleColor.Cyan => 106,
            ConsoleColor.White => 107,
            _ => 49 // default background
        };
        return Esc + code + "m";
    }

    /// <summary>
    /// Returns an ANSI truecolor (24-bit) foreground escape sequence.
    /// </summary>
    /// <param name="r">Red channel (0–255).</param>
    /// <param name="g">Green channel (0–255).</param>
    /// <param name="b">Blue channel (0–255).</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[38;2;255;128;0m").</returns>
    public static string FgRgb(byte r, byte g, byte b) => Esc + $"38;2;{r};{g};{b}m";

    /// <summary>
    /// Returns an ANSI truecolor (24-bit) background escape sequence.
    /// </summary>
    /// <param name="r">Red channel (0–255).</param>
    /// <param name="g">Green channel (0–255).</param>
    /// <param name="b">Blue channel (0–255).</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[48;2;255;128;0m").</returns>
    public static string BgRgb(byte r, byte g, byte b) => Esc + $"48;2;{r};{g};{b}m";

    /// <summary>
    /// Wraps <paramref name="text"/> in the specified 16-color foreground color
    /// and optional bold/dim/italic styling, terminated with <see cref="Reset"/>.
    /// </summary>
    /// <param name="text">The text to colorize.</param>
    /// <param name="fg">Foreground color.</param>
    /// <param name="bold">If true, applies bold styling.</param>
    /// <param name="dim">If true, applies dim styling.</param>
    /// <param name="italic">If true, applies italic styling.</param>
    /// <returns>A string with ANSI escape sequences wrapping the text.</returns>
    public static string WithColor(string text, ConsoleColor fg, bool bold = false, bool dim = false, bool italic = false)
    {
        var sb = new StringBuilder();
        if (bold) sb.Append(Bold);
        if (dim) sb.Append(Dim);
        if (italic) sb.Append(Italic);
        sb.Append(Fg(fg));
        sb.Append(text);
        sb.Append(Reset);
        return sb.ToString();
    }

    /// <summary>
    /// Wraps <paramref name="text"/> in the specified truecolor foreground
    /// and optional bold/dim/italic styling, terminated with <see cref="Reset"/>.
    /// </summary>
    /// <param name="text">The text to colorize.</param>
    /// <param name="r">Red channel (0–255).</param>
    /// <param name="g">Green channel (0–255).</param>
    /// <param name="b">Blue channel (0–255).</param>
    /// <param name="bold">If true, applies bold styling.</param>
    /// <param name="dim">If true, applies dim styling.</param>
    /// <param name="italic">If true, applies italic styling.</param>
    /// <returns>A string with ANSI escape sequences wrapping the text.</returns>
    public static string WithColorRgb(string text, byte r, byte g, byte b, bool bold = false, bool dim = false, bool italic = false)
    {
        var sb = new StringBuilder();
        if (bold) sb.Append(Bold);
        if (dim) sb.Append(Dim);
        if (italic) sb.Append(Italic);
        sb.Append(FgRgb(r, g, b));
        sb.Append(text);
        sb.Append(Reset);
        return sb.ToString();
    }
}
