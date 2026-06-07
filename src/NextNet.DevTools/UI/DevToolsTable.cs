using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// A terminal-renderable table for DevTools.
/// </summary>
public sealed class DevToolsTable
{
    private readonly string[] _headers;
    private readonly List<string[]> _rows = new();
    private readonly List<int> _columnWidths;

    /// <summary>
    /// Creates a new DevTools table with the specified column headers.
    /// </summary>
    public DevToolsTable(params string[] headers)
    {
        _headers = headers;
        _columnWidths = headers.Select(h => h.Length).ToList();
    }

    /// <summary>Add a row to the table.</summary>
    public DevToolsTable AddRow(params string[] values)
    {
        if (values.Length != _headers.Length)
            throw new ArgumentException($"Row has {values.Length} columns, expected {_headers.Length}");

        _rows.Add(values);
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i].Length > _columnWidths[i])
                _columnWidths[i] = values[i].Length;
        }

        return this;
    }

    /// <summary>Render the table as a string for terminal output.</summary>
    public string Render()
    {
        var sb = new StringBuilder();

        // Calculate column widths (capped at 40 chars for safety)
        var widths = _columnWidths.Select(w => Math.Min(w + 2, 40)).ToArray();
        var totalWidth = widths.Sum() + widths.Length + 1;

        // Header separator
        sb.AppendLine(new string('─', totalWidth));

        // Header row
        sb.Append('│');
        for (int i = 0; i < _headers.Length; i++)
        {
            var header = _headers[i].PadRight(widths[i] - 1);
            sb.Append($" {header}│");
        }
        sb.AppendLine();

        // Header separator
        sb.AppendLine(new string('─', totalWidth));

        // Data rows
        foreach (var row in _rows)
        {
            sb.Append('│');
            for (int i = 0; i < row.Length; i++)
            {
                var cell = row[i].PadRight(widths[i] - 1);
                sb.Append($" {cell}│");
            }
            sb.AppendLine();
        }

        // Footer separator
        sb.AppendLine(new string('─', totalWidth));

        return sb.ToString();
    }

    /// <summary>Render the table to the console.</summary>
    public void RenderToConsole()
    {
        System.Console.Write(Render());
    }
}
