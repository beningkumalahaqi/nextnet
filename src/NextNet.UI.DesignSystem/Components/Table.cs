using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.DesignSystem.Components;

/// <summary>
/// Standard implementation of <see cref="ITable"/> that renders tabular data
/// with sortable columns, striped rows, and hover highlighting.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Table"/> renders a <c>&lt;table&gt;</c> element with <c>&lt;thead&gt;</c>
/// and <c>&lt;tbody&gt;</c> sections. The following CSS classes are applied:
/// </para>
/// <list type="bullet">
///   <item><description><c>table</c> — base class</description></item>
///   <item><description><c>table-striped</c> — alternating row colors</description></item>
///   <item><description><c>table-hoverable</c> — row hover highlight</description></item>
///   <item><description><c>table-sortable</c> — when columns support sorting</description></item>
///   <item><description><c>table-sort-icon</c> — sort indicator in header</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var table = new Table
/// {
///     Columns = new[]
///     {
///         new TableColumn("name", "Name", Sortable: true),
///         new TableColumn("age", "Age", Sortable: true)
///     },
///     Rows = new[]
///     {
///         new Dictionary&lt;string, object?&gt; { ["name"] = "Alice", ["age"] = 30 },
///         new Dictionary&lt;string, object?&gt; { ["name"] = "Bob", ["age"] = 25 }
///     },
///     Striped = true,
///     Hoverable = true
/// };
/// var html = table.Render(context);
/// </code>
/// </example>
public sealed class Table : ITable, IRenderableComponent
{
    /// <summary>
    /// Gets or sets the column definitions for the table.
    /// </summary>
    public IReadOnlyList<TableColumn>? Columns { get; init; }

    /// <summary>
    /// Gets or sets the row data, where each row is a dictionary of column key to cell value.
    /// </summary>
    public IReadOnlyList<IReadOnlyDictionary<string, object?>>? Rows { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether columns support sorting.
    /// </summary>
    public bool Sortable { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether rows alternate background colors.
    /// </summary>
    public bool Striped { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether rows highlight on hover.
    /// </summary>
    public bool Hoverable { get; init; }

    /// <summary>
    /// Gets or sets the CSS class name(s) applied to the table's root element.
    /// </summary>
    public string? ClassName { get; init; }

    /// <summary>
    /// Gets or sets the inline CSS style string applied to the table's root element.
    /// </summary>
    public string? Style { get; init; }

    /// <summary>
    /// Gets or sets the unique identifier for this table instance.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the read-only list of child components. Tables typically have no children.
    /// </summary>
    public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();

    /// <summary>
    /// Renders this table component as HTML using the specified rendering context.
    /// </summary>
    /// <param name="context">The rendering context providing tokens and services.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered table.</returns>
    public IHtmlContent Render(RenderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var tableClass = "table";
        if (Striped) tableClass += " table-striped";
        if (Hoverable) tableClass += " table-hoverable";
        if (Sortable) tableClass += " table-sortable";
        if (!string.IsNullOrEmpty(ClassName)) tableClass += $" {ClassName}";

        var attrs = new Dictionary<string, string> { ["class"] = tableClass };
        if (!string.IsNullOrEmpty(Id)) attrs["id"] = Id;
        if (!string.IsNullOrEmpty(Style)) attrs["style"] = Style;

        var sections = new List<IHtmlContent>();

        // Header
        if (Columns != null && Columns.Count > 0)
        {
            var headerCells = new List<IHtmlContent>();
            foreach (var column in Columns)
            {
                var cellAttrs = new Dictionary<string, string>
                {
                    ["class"] = "table-header-cell"
                };
                if (column.Sortable) cellAttrs["data-sortable"] = "true";

                var cellContent = HtmlHelper.Text(column.Header);
                if (column.Sortable)
                {
                    cellContent = HtmlHelper.Fragment(
                        cellContent,
                        HtmlHelper.Element(
                            "span",
                            new Dictionary<string, string> { ["class"] = "table-sort-icon" }));
                }

                headerCells.Add(HtmlHelper.Element("th", cellAttrs, cellContent));
            }

            sections.Add(HtmlHelper.Element(
                "thead",
                null,
                HtmlHelper.Element("tr", null, HtmlHelper.Fragment(headerCells.ToArray()))));
        }

        // Body
        if (Rows != null && Rows.Count > 0)
        {
            var rowElements = new List<IHtmlContent>();
            foreach (var row in Rows)
            {
                var cells = new List<IHtmlContent>();
                if (Columns != null)
                {
                    foreach (var column in Columns)
                    {
                        var cellValue = row.TryGetValue(column.Key, out var val) ? val?.ToString() : "";
                        cells.Add(HtmlHelper.Element(
                            "td",
                            new Dictionary<string, string> { ["class"] = "table-cell" },
                            HtmlHelper.Text(cellValue ?? "")));
                    }
                }

                rowElements.Add(HtmlHelper.Element("tr", null, HtmlHelper.Fragment(cells.ToArray())));
            }

            sections.Add(HtmlHelper.Element(
                "tbody",
                null,
                HtmlHelper.Fragment(rowElements.ToArray())));
        }

        return HtmlHelper.Element("table", attrs, HtmlHelper.Fragment(sections.ToArray()));
    }
}
