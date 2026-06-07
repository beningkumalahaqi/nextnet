namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a single column or field in a table or collection.
/// </summary>
/// <param name="Name">The column/field name.</param>
/// <param name="DataType">The database data type (e.g., "nvarchar(100)", "ObjectId", "INTEGER").</param>
/// <param name="IsNullable">Whether the column allows null values.</param>
/// <param name="IsKey">Whether this column is part of the primary key.</param>
/// <param name="DefaultValue">The default value expression, if any.</param>
/// <param name="MaxLength">The maximum length for string types.</param>
/// <param name="IsAutoIncrement">Whether the column auto-increments.</param>
/// <param name="Comment">An optional description or comment.</param>
public sealed record SchemaColumnInfo(
    string Name,
    string DataType,
    bool IsNullable = false,
    bool IsKey = false,
    string? DefaultValue = null,
    int? MaxLength = null,
    bool IsAutoIncrement = false,
    string? Comment = null
);
