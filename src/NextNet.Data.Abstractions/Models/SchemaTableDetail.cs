namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Detailed schema information for a specific table or collection.
/// </summary>
/// <param name="Name">The table or collection name.</param>
/// <param name="Schema">The database schema.</param>
/// <param name="Type">"Table", "View", or "Collection".</param>
/// <param name="Columns">The columns/fields in the table.</param>
/// <param name="PrimaryKey">The primary key column(s), if any.</param>
/// <param name="ForeignKeys">Foreign key relationships, if any.</param>
/// <param name="Indexes">Defined indexes, if available.</param>
public sealed record SchemaTableDetail(
    string Name,
    string? Schema,
    string Type,
    IReadOnlyList<SchemaColumnInfo> Columns,
    IReadOnlyList<string>? PrimaryKey = null,
    IReadOnlyList<SchemaForeignKey>? ForeignKeys = null,
    IReadOnlyList<SchemaIndex>? Indexes = null
);
