using NextNet.DevTools.UI;

namespace NextNet.DevTools.Panels;

/// <summary>
/// Console Log panel — displays console output from the dev server, including
/// HMR status, file changes, and log messages with color-coded severity.
/// </summary>
/// <example>
/// <code>
/// var panel = new ConsolePanel(dataStore);
/// panel.Render(new TuiRenderContext(120));
/// panel.HandleInput(ConsoleKey.C); // clear logs
/// </code>
/// </example>
public sealed class ConsolePanel : IDevToolsPanel
{
    private readonly DevToolsDataStore _dataStore;

    /// <inheritdoc />
    public string Name => "Console";

    /// <inheritdoc />
    public string Icon => "📋";

    /// <summary>
    /// Creates a new ConsolePanel.
    /// </summary>
    /// <param name="dataStore">The DevTools data store providing console log entries.</param>
    public ConsolePanel(DevToolsDataStore dataStore)
    {
        _dataStore = dataStore;
    }

    /// <inheritdoc />
    public void Render(TuiRenderContext context)
    {
        var logs = _dataStore.GetConsoleLogs();

        var width = Math.Min(context.Width - 4, 80);

        System.Console.WriteLine($" Console Log — {logs.Count} entries");
        System.Console.WriteLine();

        if (logs.Count == 0)
        {
            System.Console.WriteLine("  No console output yet.");
            System.Console.WriteLine();
            return;
        }

        // Show last 50 entries
        var displayLogs = logs.TakeLast(50).ToList();

        foreach (var log in displayLogs)
        {
            var text = log.Length > width ? log[..(width - 3)] + "..." : log;

            if (text.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("fail", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.ForegroundColor = ConsoleColor.Red;
            }
            else if (text.Contains("warn", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.ForegroundColor = ConsoleColor.Yellow;
            }
            else if (text.Contains("✓", StringComparison.OrdinalIgnoreCase) ||
                     text.Contains("success", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.ForegroundColor = ConsoleColor.Green;
            }
            else if (text.Contains("→", StringComparison.OrdinalIgnoreCase) ||
                     text.Contains("->", StringComparison.OrdinalIgnoreCase))
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
            }
            else
            {
                System.Console.ForegroundColor = ConsoleColor.Gray;
            }

            System.Console.WriteLine($"  {text}");
        }

        System.Console.ResetColor();
        System.Console.WriteLine();
        System.Console.ForegroundColor = ConsoleColor.DarkGray;
        System.Console.WriteLine($"  Showing last {displayLogs.Count} entries. Press C to clear.");
        System.Console.ResetColor();
    }

    /// <inheritdoc />
    public void HandleInput(ConsoleKey key)
    {
        switch (key)
        {
            case ConsoleKey.C:
                _dataStore.ClearConsoleLogs();
                break;
        }
    }
}
