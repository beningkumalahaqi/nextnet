namespace NextNet.UI.Abstractions.Components;

/// <summary>
/// Defines a single column definition within a table.
/// </summary>
/// <param name="Key">The unique key used to look up cell data from a row.</param>
/// <param name="Header">The display text for the column header.</param>
/// <param name="Sortable">Whether the column supports sorting.</param>
public sealed record TableColumn(
    string Key,
    string Header,
    bool Sortable = false);

/// <summary>
/// Defines the contract for a table UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ITable"/> extends <see cref="IComponent"/> with properties
/// for displaying tabular data, including column definitions, row data,
/// sortability, and visual options like striped rows and hover highlighting.
/// </para>
/// <para>
/// Each row is a dictionary mapping column keys to cell values. When
/// <see cref="Sortable"/> is <c>true</c>, column headers render sort controls
/// and the table should support interactive sorting.
/// </para>
/// </remarks>
public interface ITable : IComponent
{
    /// <summary>
    /// Gets the column definitions for the table.
    /// </summary>
    IReadOnlyList<TableColumn>? Columns { get; }

    /// <summary>
    /// Gets the row data, where each row is a dictionary of column key to cell value.
    /// </summary>
    IReadOnlyList<IReadOnlyDictionary<string, object?>>? Rows { get; }

    /// <summary>
    /// Gets a value indicating whether columns support sorting.
    /// </summary>
    bool Sortable { get; }

    /// <summary>
    /// Gets a value indicating whether rows alternate background colors
    /// for improved readability.
    /// </summary>
    bool Striped { get; }

    /// <summary>
    /// Gets a value indicating whether rows highlight on hover.
    /// </summary>
    bool Hoverable { get; }
}
