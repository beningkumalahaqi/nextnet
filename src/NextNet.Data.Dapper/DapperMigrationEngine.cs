using NextNet.Data.Dapper.Internal;

namespace NextNet.Data.Dapper;

/// <summary>
/// Dapper implementation of <see cref="IMigrationEngine"/>.
/// Manages raw SQL migration files with a <c>__NextNetMigrations</c>
/// history table for tracking applied migrations.
/// </summary>
/// <remarks>
/// <para>
/// Unlike EF Core which auto-generates migrations from model changes,
/// Dapper uses hand-authored SQL migration files stored in the
/// <c>Migrations/</c> directory. Each file is named with a timestamp
/// prefix: <c>YYYYMMDDHHmmss_description.sql</c>.
/// </para>
/// <para>
/// <see cref="ApplyAsync"/> discovers pending migration files (those not
/// yet recorded in the history table), wraps each in a transaction, executes
/// the SQL, and records the migration name on success.
/// </para>
/// <para>
/// <see cref="RollbackAsync"/> reads the most recent migration from the
/// history table and executes the corresponding rollback script
/// (<c>*_description.down.sql</c>) if present.
/// </para>
/// </remarks>
public sealed class DapperMigrationEngine : IMigrationEngine
{
    private readonly DapperConnectionManager _connectionManager;
    private readonly MigrationConfig? _config;
    private readonly ILogger<DapperMigrationEngine> _logger;
    private readonly MigrationHistoryRepository _historyRepository;
    private readonly MigrationFileSystem _fileSystem;
    private readonly string _connectionName;

    /// <summary>
    /// Initializes a new instance of <see cref="DapperMigrationEngine"/>.
    /// </summary>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="config">Optional migration configuration (directory, history table name, timeout).</param>
    /// <param name="connectionName">The name of the connection to use. Defaults to "Default".</param>
    /// <param name="logger">Optional logger for migration operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionManager"/> is null.</exception>
    public DapperMigrationEngine(
        DapperConnectionManager connectionManager,
        MigrationConfig? config = null,
        string? connectionName = null,
        ILogger<DapperMigrationEngine>? logger = null)
    {
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _config = config;
        _connectionName = connectionName ?? "Default";
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<DapperMigrationEngine>();

        var migrationsDir = _config?.Directory ?? "Migrations";
        var historyTableName = _config?.HistoryTableName ?? "__NextNetMigrations";

        _fileSystem = new MigrationFileSystem(migrationsDir, _logger);
        _historyRepository = new MigrationHistoryRepository(
            _connectionManager, historyTableName, _connectionName, _logger);
    }

    /// <summary>
    /// Creates a new SQL migration file with the specified name.
    /// Produces both an up script (<c>{timestamp}_{name}.sql</c>) and
    /// a down script (<c>{timestamp}_{name}.down.sql</c>).
    /// </summary>
    /// <param name="name">A descriptive name for the migration (e.g., "AddUserTable").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public Task<MigrationResult> AddMigrationAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Migration name must not be null or empty.");
        }

        _logger.LogInformation("Creating migration '{MigrationName}'...", name);

        try
        {
            var (upPath, downPath) = _fileSystem.CreateMigrationFiles(name);

            _logger.LogInformation("Migration '{MigrationName}' created successfully.", name);
            return Task.FromResult(new MigrationResult(
                true,
                $"Migration '{name}' created successfully. Files: {upPath}, {downPath}",
                MigrationName: name));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create migration '{MigrationName}'.", name);
            return Task.FromResult(new MigrationResult(
                false,
                $"Failed to create migration '{name}': {ex.Message}",
                Errors: new[] { ex.ToString() }));
        }
    }

    /// <summary>
    /// Discovers and applies all pending SQL migration files not yet recorded
    /// in the <c>__NextNetMigrations</c> history table. Each migration is
    /// executed in a separate transaction.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> with the count of applied migrations.</returns>
    public async Task<MigrationResult> ApplyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying pending migrations...");

        try
        {
            // Ensure history table exists
            await _historyRepository.EnsureHistoryTableExistsAsync(cancellationToken);

            // Discover applied migrations
            var appliedMigrations = await _historyRepository.GetAppliedMigrationsAsync(cancellationToken);

            // Discover pending migration files
            var pendingMigrations = _fileSystem.GetPendingMigrations(appliedMigrations);

            if (pendingMigrations.Count == 0)
            {
                _logger.LogInformation("No pending migrations to apply.");
                return new MigrationResult(true, "No pending migrations to apply.", MigrationsApplied: 0);
            }

            var appliedCount = 0;
            var errors = new List<string>();

            foreach (var migrationFile in pendingMigrations)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var migrationName = MigrationFileSystem.GetMigrationName(migrationFile);

                _logger.LogInformation("Applying migration '{MigrationName}'...", migrationName);

                using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);
                using var transaction = conn.BeginTransaction();

                try
                {
                    var sql = await _fileSystem.ReadMigrationScriptAsync(migrationFile);

                    await conn.ExecuteAsync(sql, transaction: transaction, commandTimeout: _config?.TimeoutSeconds ?? 60);

                    await _historyRepository.RecordMigrationAsync(migrationName, conn, transaction);

                    transaction.Commit();
                    appliedCount++;
                    _logger.LogInformation("Migration '{MigrationName}' applied successfully.", migrationName);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    var errorMsg = $"Migration '{migrationName}' failed: {ex.Message}";
                    _logger.LogError(ex, "Migration '{MigrationName}' failed.", migrationName);
                    errors.Add(errorMsg);
                }
            }

            _logger.LogInformation("Applied {Count} migration(s) successfully.", appliedCount);
            return new MigrationResult(
                true,
                $"Applied {appliedCount} migration(s) successfully.",
                MigrationsApplied: appliedCount,
                Errors: errors.Count > 0 ? errors.AsReadOnly() : null);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Migration application was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply migrations.");
            return new MigrationResult(
                false,
                $"Failed to apply migrations: {ex.Message}",
                Errors: new[] { ex.ToString() });
        }
    }

    /// <summary>
    /// Rolls back the most recently applied migration by executing its
    /// down script (<c>*.down.sql</c>). If no down script exists, returns
    /// a failure result with a descriptive message.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public async Task<MigrationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rolling back the last migration...");

        try
        {
            await _historyRepository.EnsureHistoryTableExistsAsync(cancellationToken);

            var lastMigrationName = await _historyRepository.GetLastMigrationNameAsync(cancellationToken);

            if (lastMigrationName is null)
            {
                _logger.LogInformation("No migrations to roll back.");
                return new MigrationResult(true, "No migrations to roll back.", MigrationsApplied: 0);
            }

            // Find the up migration file to derive the down script path
            var migrationsDir = Path.GetFullPath(_config?.Directory ?? "Migrations");
            var upFilePath = Directory.GetFiles(migrationsDir, $"{lastMigrationName}.sql").FirstOrDefault();

            if (upFilePath is null)
            {
                return new MigrationResult(
                    false,
                    $"Cannot find migration file for '{lastMigrationName}'.",
                    Errors: new[] { $"File '{lastMigrationName}.sql' not found in '{migrationsDir}'." });
            }

            var downPath = _fileSystem.GetDownScriptPath(upFilePath);
            if (downPath is null)
            {
                return new MigrationResult(
                    false,
                    $"No rollback script found for migration '{lastMigrationName}'. " +
                    "Create a *.down.sql file to enable rollback.",
                    MigrationName: lastMigrationName);
            }

            var downSql = await _fileSystem.ReadDownScriptAsync(downPath);
            if (string.IsNullOrWhiteSpace(downSql))
            {
                return new MigrationResult(
                    false,
                    $"Down script for '{lastMigrationName}' is empty.",
                    MigrationName: lastMigrationName);
            }

            using var conn = await _connectionManager.OpenConnectionAsync(_connectionName, cancellationToken);
            using var transaction = conn.BeginTransaction();

            try
            {
                await conn.ExecuteAsync(downSql, transaction: transaction, commandTimeout: _config?.TimeoutSeconds ?? 60);

                await _historyRepository.RemoveLastMigrationAsync(conn, transaction);

                transaction.Commit();
                _logger.LogInformation("Migration '{MigrationName}' rolled back successfully.", lastMigrationName);
                return new MigrationResult(
                    true,
                    $"Migration '{lastMigrationName}' rolled back successfully.",
                    MigrationsApplied: 1,
                    MigrationName: lastMigrationName);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Migration rollback was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rollback migration.");
            return new MigrationResult(
                false,
                $"Failed to rollback migration: {ex.Message}",
                Errors: new[] { ex.ToString() });
        }
    }
}
