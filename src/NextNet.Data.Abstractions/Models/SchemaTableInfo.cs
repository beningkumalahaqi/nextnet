namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a single table or collection in the database.
/// </summary>
/// <param name="Name">The table or collection name.</param>
/// <param name="Schema">The database schema (relational only; null for MongoDB).</param>
/// <param name="Type">The object type: "Table", "View", or "Collection".</param>
/// <param name="ColumnCount">The number of columns/fields.</param>
/// <param name="EstimatedRowCount">An estimated row/document count, if available.</param>
/// <param name="IsSystem">Whether this is a system/internal table/collection.</param>
public sealed record SchemaTableInfo(
    string Name,
    string? Schema,
    string Type,
    int ColumnCount,
    long? EstimatedRowCount = null,
    bool IsSystem = false
);
