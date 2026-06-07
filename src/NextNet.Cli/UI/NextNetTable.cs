using Spectre.Console;
using Spectre.Console.Rendering;

namespace NextNet.Cli.UI;

/// <summary>
/// Wraps Spectre.Console's <see cref="Table"/> with NextNet branding and
/// consistent border/header styling. Provides factory methods for common table variants.
/// </summary>
public sealed class NextNetTable
{
    private readonly Table _table;
    private readonly OutputMode _mode;

    /// <summary>
    /// Creates a new table with NextNet-branded borders and headers.
    /// </summary>
    /// <param name="headers">Column headers.</param>
    /// <param name="mode">Output mode (Color or Plain).</param>
    public NextNetTable(string[] headers, OutputMode mode = OutputMode.Color)
    {
        _mode = mode;
        _table = new Table()
            .Border(_mode == OutputMode.Plain ? TableBorder.Ascii : TableBorder.Rounded)
            .BorderColor(_mode == OutputMode.Color ? Theme.BorderColor : Color.Default)
            .SafeBorder();

        foreach (var header in headers)
        {
            var headerStyle = _mode == OutputMode.Color
                ? new Style(foreground: Theme.TableHeaderColor, decoration: Decoration.Bold)
                : Style.Plain;

            var column = new TableColumn(new Markup(header, headerStyle))
                .NoWrap();
            _table.AddColumn(column);
        }
    }

    /// <summary>Add a row with string values.</summary>
    public void AddRow(params string[] values) => _table.AddRow(values);

    /// <summary>Add a row with markup values (styled cells).</summary>
    public void AddRow(params IRenderable[] values) => _table.AddRow(values);

    /// <summary>Add a separator line.</summary>
    public void AddSeparator() => _table.AddEmptyRow();

    /// <summary>
    /// Add a total/summary row with bold label and highlighted value.
    /// </summary>
    public void AddTotalRow(string label, string value)
    {
        _table.AddEmptyRow();
        _table.AddRow(
            new Markup($"[bold {Theme.NextNetTealHex}]{label}[/]"),
            new Markup($"[bold {Theme.SuccessHex}]{value}[/]"));
    }

    /// <summary>
    /// Add a step row with checkmark and duration.
    /// </summary>
    public void AddStepRow(string stepName, string duration, bool completed = true)
    {
        var stepMarkup = completed
            ? new Markup($"[bold {Theme.SuccessHex}]\u2713[/] {stepName}")
            : new Markup($"[bold {Theme.ErrorHex}]\u2717[/] {stepName}");
        _table.AddRow(stepMarkup, new Text(duration, Theme.MutedStyle));
    }

    /// <summary>Expose the underlying Spectre.Console table for custom use.</summary>
    public Table GetSpectreTable() => _table;

    /// <summary>Render the table to the console.</summary>
    public void Render(IAnsiConsole console) => console.Write(_table);

    // ── Factory methods ──────────────────────────────────────────────

    /// <summary>Creates a build summary table with Step and Duration columns.</summary>
    public static NextNetTable BuildSummary(OutputMode mode = OutputMode.Color)
        => new(new[] { "Step", "Duration" }, mode);

    /// <summary>Creates a route listing table.</summary>
    public static NextNetTable Routes(OutputMode mode = OutputMode.Color)
        => new(new[] { "Method", "Path", "File", "Type" }, mode);

    /// <summary>Creates a plugin list table.</summary>
    public static NextNetTable Plugins(OutputMode mode = OutputMode.Color)
        => new(new[] { "Name", "Version", "Status" }, mode);

    /// <summary>Creates an info/environment table.</summary>
    public static NextNetTable Info(OutputMode mode = OutputMode.Color)
        => new(new[] { "Key", "Value" }, mode);

    /// <summary>Creates an output file listing table.</summary>
    public static NextNetTable OutputFiles(OutputMode mode = OutputMode.Color)
        => new(new[] { "Path", "Size" }, mode);
}
