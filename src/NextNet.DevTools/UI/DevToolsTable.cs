using System.Text;

namespace NextNet.DevTools.UI;

/// <summary>
/// A terminal-renderable table for DevTools.
/// Supports headers, data rows, automatic column width calculation, and rendering to string or console.
/// </summary>
/// <example>
/// <code>
/// var table = new DevToolsTable("Name", "Type", "Renders", "Avg (ms)");
/// table.AddRow("Header", "component", "150", "5");
/// table.AddRow("Footer", "component", "80", "3");
/// Console.Write(table.Render());
/// </code>
/// </example>
public sealed class DevToolsTable
{
    private readonly string[] _headers;
    private readonly List<string[]> _rows = new();
    private readonly List<int> _columnWidths;

    /// <summary>
    /// Creates a new DevTools table with the specified column headers.
    /// </summary>
    /// <param name="headers">Column header names (variable length).</param>
    public DevToolsTable(params string[] headers)
    {
        _headers = headers;
        _columnWidths = headers.Select(h => h.Length).ToList();
    }

    /// <summary>Add a row to the table. The number of values must match the number of headers.</summary>
    /// <param name="values">Cell values for the row.</param>
    /// <returns>The table instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when the number of values does not match the number of headers.</exception>
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
