namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a foreign key relationship between tables.
/// </summary>
/// <param name="ConstraintName">The FK constraint name.</param>
/// <param name="ColumnName">The source column in the current table.</param>
/// <param name="ReferencedTable">The referenced table.</param>
/// <param name="ReferencedSchema">The referenced table's schema.</param>
/// <param name="ReferencedColumn">The referenced column (typically primary key).</param>
/// <param name="OnDelete">The delete action (e.g., "CASCADE", "SET NULL").</param>
public sealed record SchemaForeignKey(
    string ConstraintName,
    string ColumnName,
    string ReferencedTable,
    string? ReferencedSchema,
    string ReferencedColumn,
    string? OnDelete = null
);
