using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// Helper for rendering bordered panels with titles in the terminal.
/// </summary>
public static class DevToolsPanel
{
    /// <summary>
    /// Creates a bordered panel string with the given title and content lines.
    /// </summary>
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
    public static void Render(string title, string content, int width = 80)
    {
        System.Console.Write(Create(title, content, width));
        System.Console.WriteLine();
    }

    /// <summary>
    /// Creates a status bar string for the bottom of the TUI.
    /// </summary>
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
