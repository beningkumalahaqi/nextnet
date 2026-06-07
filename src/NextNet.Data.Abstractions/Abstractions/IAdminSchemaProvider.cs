using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Provides schema introspection for a connected database.
/// Each data provider implements this to expose tables, columns, and
/// relationships for CLI exploration and admin scaffolding.
/// </summary>
/// <remarks>
/// <para>
/// Schema providers are registered alongside data providers and resolved
/// by the CLI for database exploration commands. They must be able to
/// operate without a running application — connecting directly via the
/// configured connection string.
/// </para>
/// </remarks>
public interface IAdminSchemaProvider
{
    /// <summary>
    /// Gets the provider name this schema provider supports
    /// (e.g., "EntityFramework", "Dapper", "MongoDB").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Lists all tables (relational) or collections (document) in the database.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    /// <param name="includeViews">Whether to include database views in results.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of schema table descriptors.</returns>
    Task<IReadOnlyList<SchemaTableInfo>> ListTablesAsync(
        string connectionString,
        bool includeViews = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets detailed schema information for a specific table or collection.
    /// </summary>
    /// <param name="connectionString">The connection string to use.</param>
    /// <param name="tableName">The table or collection name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>Detailed schema information including columns and relationships.</returns>
    Task<SchemaTableDetail> GetTableDetailAsync(
        string connectionString,
        string tableName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests whether the provider can connect to the specified database.
    /// </summary>
    /// <param name="connectionString">The connection string to test.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>True if the connection succeeds; false otherwise.</returns>
    Task<bool> TestConnectionAsync(
        string connectionString,
        CancellationToken cancellationToken = default);
}
