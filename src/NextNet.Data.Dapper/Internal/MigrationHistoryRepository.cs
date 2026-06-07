using Microsoft.Data.SqlClient;

namespace NextNet.Data.Dapper.Internal;

/// <summary>
/// Manages the <c>__NextNetMigrations</c> history table for tracking applied migrations.
/// </summary>
/// <remarks>
/// <para>
/// This repository handles creating the history table if it does not exist,
/// querying applied migration names, recording new migrations, and removing
/// rolled-back migration records.
/// </para>
/// </remarks>
internal sealed class MigrationHistoryRepository
{
    private readonly DapperConnectionManager _connectionManager;
    private readonly string _historyTableName;
    private readonly string _connectionName;
    private readonly ILogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MigrationHistoryRepository"/>.
    /// </summary>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="historyTableName">The name of the history table. Defaults to "__NextNetMigrations".</param>
    /// <param name="connectionName">The connection name to use.</param>
    /// <param name="logger">Optional logger.</param>
    internal MigrationHistoryRepository(
        DapperConnectionManager connectionManager,
        string historyTableName,
        string connectionName,
        ILogger? logger = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _historyTableName = string.IsNullOrWhiteSpace(historyTableName) ? "__NextNetMigrations" : historyTableName;
        _connectionName = connectionName ?? "Default";
        _logger = logger;
    }

    /// <summary>
    /// Ensures the migration history table exists, creating it if necessary.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    internal async Task EnsureHistoryTableExistsAsync(CancellationToken cancellationToken = default)
    {
        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);

        var sql = $@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = @TableName)
            BEGIN
                CREATE TABLE [{_historyTableName}] (
                    [Id] INT IDENTITY(1,1) PRIMARY KEY,
                    [MigrationName] NVARCHAR(255) NOT NULL,
                    [AppliedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
                )
            END";

        _logger?.LogDebug("Ensuring migration history table '{TableName}' exists...", _historyTableName);
        await conn.ExecuteAsync(sql, new { TableName = _historyTableName }, commandTimeout: 30);
    }

    /// <summary>
    /// Gets the list of migration names already applied.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A set of applied migration names.</returns>
    internal async Task<HashSet<string>> GetAppliedMigrationsAsync(CancellationToken cancellationToken = default)
    {
        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);

        var sql = $"SELECT [MigrationName] FROM [{_historyTableName}] ORDER BY [AppliedAt] ASC";
        var names = await conn.QueryAsync<string>(sql, commandTimeout: 30);
        return new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Records a migration as applied.
    /// </summary>
    /// <param name="migrationName">The name of the migration file.</param>
    /// <param name="connection">The open SQL connection to use (must be within a transaction).</param>
    /// <param name="transaction">The transaction to use.</param>
    internal async Task RecordMigrationAsync(string migrationName, SqlConnection connection, SqlTransaction transaction)
    {
        var sql = $"INSERT INTO [{_historyTableName}] ([MigrationName], [AppliedAt]) VALUES (@MigrationName, GETUTCDATE())";
        await connection.ExecuteAsync(sql, new { MigrationName = migrationName }, transaction: transaction, commandTimeout: 30);
        _logger?.LogDebug("Recorded migration '{MigrationName}' in history table.", migrationName);
    }

    /// <summary>
    /// Removes the most recently applied migration record.
    /// </summary>
    /// <param name="connection">The open SQL connection to use (must be within a transaction).</param>
    /// <param name="transaction">The transaction to use.</param>
    internal async Task RemoveLastMigrationAsync(SqlConnection connection, SqlTransaction transaction)
    {
        var sql = $@"
            DELETE FROM [{_historyTableName}]
            WHERE [MigrationName] = (
                SELECT TOP 1 [MigrationName]
                FROM [{_historyTableName}]
                ORDER BY [AppliedAt] DESC
            )";
        await connection.ExecuteAsync(sql, transaction: transaction, commandTimeout: 30);
    }

    /// <summary>
    /// Gets the most recently applied migration name.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The last applied migration name, or null if none.</returns>
    internal async Task<string?> GetLastMigrationNameAsync(CancellationToken cancellationToken = default)
    {
        using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);

        var sql = $"SELECT TOP 1 [MigrationName] FROM [{_historyTableName}] ORDER BY [AppliedAt] DESC";
        return await conn.QueryFirstOrDefaultAsync<string>(sql, commandTimeout: 30);
    }
}
