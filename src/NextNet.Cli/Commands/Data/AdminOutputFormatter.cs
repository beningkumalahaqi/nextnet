using System.Text;
using System.Text.Json;
using NextNet.Cli.UI;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Formats database schema information for CLI display.
/// Supports tree view, table view, and JSON output modes.
/// </summary>
internal static class AdminOutputFormatter
{
    /// <summary>
    /// Formats a list of tables into a tree/table display string.
    /// </summary>
    public static string FormatTableList(IReadOnlyList<SchemaTableInfo> tables, string connectionName, string? databaseName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Connection: {connectionName}");
        if (databaseName is not null)
            sb.AppendLine($"Database:   {databaseName}");

        sb.AppendLine();

        var regularTables = tables.Where(t => t.Type == "Table" && !t.IsSystem).ToList();
        var views = tables.Where(t => t.Type == "View").ToList();
        var systemTables = tables.Where(t => t.IsSystem).ToList();

        if (regularTables.Count > 0)
        {
            sb.AppendLine($"Tables ({regularTables.Count}):");
            foreach (var table in regularTables)
            {
                var rowCount = table.EstimatedRowCount.HasValue
                    ? FormatRowCount(table.EstimatedRowCount.Value)
                    : "";
                sb.AppendLine($"  {table.Name,-30} {table.ColumnCount} cols  {rowCount}");
            }
            sb.AppendLine();
        }

        if (views.Count > 0)
        {
            sb.AppendLine($"Views ({views.Count}):");
            foreach (var view in views)
            {
                sb.AppendLine($"  {view.Name,-30} {view.ColumnCount} cols");
            }
            sb.AppendLine();
        }

        if (systemTables.Count > 0)
        {
            sb.AppendLine($"System Tables ({systemTables.Count}):");
            foreach (var table in systemTables)
            {
                sb.AppendLine($"  {table.Name,-30} {table.ColumnCount} cols");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats detailed table information into a structured display string.
    /// </summary>
    public static string FormatTableDetail(SchemaTableDetail detail)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"Table: {detail.Name}");
        if (detail.Schema is not null)
            sb.AppendLine($"  Schema:    {detail.Schema}");
        sb.AppendLine($"  Type:      {detail.Type}");
        sb.AppendLine();

        // Columns
        if (detail.Columns.Count > 0)
        {
            sb.AppendLine($"  Columns ({detail.Columns.Count}):");
            foreach (var col in detail.Columns)
            {
                var flags = new StringBuilder();
                if (col.IsKey) flags.Append("PK");
                if (col.IsAutoIncrement) flags.Append(flags.Length > 0 ? ", AUTOINCREMENT" : "AUTOINCREMENT");
                if (col.IsNullable) flags.Append(flags.Length > 0 ? ", NULL" : "NULL");
                else flags.Append(flags.Length > 0 ? ", NOT NULL" : "NOT NULL");

                var defaultStr = col.DefaultValue is not null ? $"  DEFAULT {col.DefaultValue}" : "";
                var maxLenStr = col.MaxLength.HasValue ? $"({col.MaxLength.Value})" : "";

                sb.AppendLine($"    {col.Name,-25} {col.DataType}{maxLenStr,-15} {flags,-20} {defaultStr}");
            }
            sb.AppendLine();
        }

        // Indexes
        if (detail.Indexes is { Count: > 0 })
        {
            sb.AppendLine($"  Indexes ({detail.Indexes.Count}):");
            foreach (var idx in detail.Indexes)
            {
                var cols = string.Join(", ", idx.Columns);
                var flags = new StringBuilder();
                if (idx.IsPrimary) flags.Append("PRIMARY");
                if (idx.IsUnique) flags.Append(flags.Length > 0 ? ", UNIQUE" : "UNIQUE");
                sb.AppendLine($"    {idx.IndexName,-30} {cols,-20} {flags}");
            }
            sb.AppendLine();
        }

        // Foreign Keys
        if (detail.ForeignKeys is { Count: > 0 })
        {
            sb.AppendLine($"  Foreign Keys ({detail.ForeignKeys.Count}):");
            foreach (var fk in detail.ForeignKeys)
            {
                var refSchema = fk.ReferencedSchema is not null ? $"{fk.ReferencedSchema}." : "";
                var onDelete = fk.OnDelete is not null ? $"  {fk.OnDelete}" : "";
                sb.AppendLine($"    {fk.ConstraintName,-35} {fk.ColumnName} → {refSchema}{fk.ReferencedTable}({fk.ReferencedColumn}){onDelete}");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Serializes schema data to JSON format for machine-readable output.
    /// </summary>
    public static string FormatJson(object data)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        return JsonSerializer.Serialize(data, options);
    }

    /// <summary>
    /// Formats a tree display for the NextNetTree component.
    /// </summary>
    public static NextNetTree BuildTreeView(IReadOnlyList<SchemaTableInfo> tables, string connectionName)
    {
        var tree = new NextNetTree($"Database Explorer: {connectionName}");

        var regularTables = tables.Where(t => t.Type == "Table" && !t.IsSystem).ToList();
        var views = tables.Where(t => t.Type == "View").ToList();

        if (regularTables.Count > 0)
        {
            var tableNode = tree.AddNode($"Tables ({regularTables.Count})");
            foreach (var table in regularTables)
            {
                var rowCount = table.EstimatedRowCount.HasValue
                    ? $" ({table.EstimatedRowCount.Value} rows)"
                    : "";
                tableNode.AddChild($"{table.Name}  [{table.ColumnCount} cols]{rowCount}");
            }
        }

        if (views.Count > 0)
        {
            var viewNode = tree.AddNode($"Views ({views.Count})");
            foreach (var view in views)
            {
                viewNode.AddChild($"{view.Name}  [{view.ColumnCount} cols]");
            }
        }

        return tree;
    }

    private static string FormatRowCount(long count)
    {
        return count switch
        {
            >= 1_000_000 => $"{count / 1_000_000.0:F1}M rows",
            >= 1_000 => $"{count / 1_000.0:F1}K rows",
            _ => $"{count} rows"
        };
    }
}
