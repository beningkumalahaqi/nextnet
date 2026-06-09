using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// Helper for rendering bordered panels with titles in the terminal.
/// Provides create and render methods for panels with title, content, and status bar sections.
/// </summary>
/// <example>
/// <code>
/// // Create a bordered panel string
/// var panel = DevToolsPanel.Create("My Panel", "Content line 1\nContent line 2", 40);
/// Console.Write(panel);
///
/// // Render directly to console
/// DevToolsPanel.Render("Status", "Running...", 60);
///
/// // Create a status bar
/// var statusBar = DevToolsPanel.CreateStatusBar("3 routes active", 60);
/// </code>
/// </example>
public static class DevToolsPanel
{
    /// <summary>
    /// Creates a bordered panel string with the given title and content lines.
    /// </summary>
    /// <param name="title">The panel title displayed in the top border.</param>
    /// <param name="content">Multi-line content to display inside the panel.</param>
    /// <param name="width">Total width of the panel in characters (default 80).</param>
    /// <returns>A string with box-drawing characters forming the panel.</returns>
    public static string Create(string title, string content, int width = 80)
    {
        var sb = new StringBuilder();
        var innerWidth = width - 4; // padding

        // Top border with title
        sb.Append('┌');
        sb.Append(' ');
        sb.Append(title);
        var titleLen = title.Length + 2;
        sb.Append(new string('─', Math.Max(0, width - titleLen - 1)));
        sb.Append('┐');
        sb.AppendLine();

        // Content lines
        var lines = content.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd('\r');
            sb.Append("│ ");
            sb.Append(trimmed.PadRight(innerWidth));
            sb.Append(" │");
            sb.AppendLine();
        }

        // Bottom border
        sb.Append('└');
        sb.Append(new string('─', width - 2));
        sb.Append('┘');

        return sb.ToString();
    }

    /// <summary>
    /// Renders a bordered panel with title and content to the console.
    /// </summary>
    /// <param name="title">The panel title.</param>
    /// <param name="content">Multi-line content.</param>
    /// <param name="width">Total width in characters (default 80).</param>
    public static void Render(string title, string content, int width = 80)
    {
        System.Console.Write(Create(title, content, width));
        System.Console.WriteLine();
    }

    /// <summary>
    /// Creates a status bar string for the bottom of the TUI.
    /// </summary>
    /// <param name="statusText">The status text to display.</param>
    /// <param name="width">Total width of the status bar in characters (default 80).</param>
    /// <returns>A string with box-drawing characters forming the status bar.</returns>
    public static string CreateStatusBar(string statusText, int width = 80)
    {
        var sb = new StringBuilder();
        sb.Append('├');
        sb.Append(new string('─', width - 2));
        sb.Append('┤');
        sb.AppendLine();

        sb.Append("│ ");
        sb.Append(statusText.PadRight(width - 4));
        sb.Append(" │");
        sb.AppendLine();

        sb.Append('└');
        sb.Append(new string('─', width - 2));
        sb.Append('┘');

        return sb.ToString();
    }
}
