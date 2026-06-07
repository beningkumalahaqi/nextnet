namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a database index.
/// </summary>
/// <param name="IndexName">The index name.</param>
/// <param name="Columns">The indexed columns.</param>
/// <param name="IsUnique">Whether the index enforces uniqueness.</param>
/// <param name="IsPrimary">Whether this is the primary key index.</param>
public sealed record SchemaIndex(
    string IndexName,
    IReadOnlyList<string> Columns,
    bool IsUnique = false,
    bool IsPrimary = false
);
