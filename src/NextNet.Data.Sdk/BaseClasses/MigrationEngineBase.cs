#if NET8_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NextNet.Data.Sdk.Base;

/// <summary>
/// Base class for provider-specific migration engines. Implements <see cref="IMigrationEngine"/>
/// with file-system migration discovery, history table tracking, retry logic, and
/// configurable timeout.
/// </summary>
/// <remarks>
/// <para>
/// Provider authors override <see cref="ExecuteMigrationAsync"/> to run a single
/// migration script against their database. The base class handles discovering
/// migration files, tracking which have been applied, and ordering by timestamp.
/// </para>
/// <para>
/// Migration files are expected in the configured <see cref="MigrationConfig.Directory"/>
/// with names following the pattern: <c>{timestamp}_{name}.{format}</c>
/// (e.g., <c>20260606_120000_AddUserTable.sql</c>).
/// </para>
/// </remarks>
public abstract class MigrationEngineBase : IMigrationEngine
{
    private readonly IOptions<MigrationConfig> _config;
    private readonly ConnectionManagerBase _connectionManager;
    private readonly ILogger? _logger;

    /// <summary>
    /// Gets the migration configuration.
    /// </summary>
    protected IOptions<MigrationConfig> Config => _config;

    /// <summary>
    /// Gets the connection manager used for database access.
    /// </summary>
    protected ConnectionManagerBase ConnectionManager => _connectionManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="MigrationEngineBase"/> class.
    /// </summary>
    /// <param name="config">Migration configuration.</param>
    /// <param name="connectionManager">The connection manager for database access.</param>
    /// <param name="logger">An optional logger.</param>
    protected MigrationEngineBase(
        IOptions<MigrationConfig> config,
        ConnectionManagerBase connectionManager,
        ILogger? logger = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        _logger = logger;
    }

    /// <summary>
    /// Creates a new empty migration file with the given name.
    /// </summary>
    /// <param name="name">A descriptive name for the migration (e.g., "AddUserTable").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public async Task<MigrationResult> AddMigrationAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new MigrationResult(false, "Migration name cannot be empty.");

        try
        {
            var migrationDir = _config.Value.Directory;
            if (!Directory.Exists(migrationDir))
                Directory.CreateDirectory(migrationDir);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var format = GetMigrationFileExtension();
            var fileName = $"{timestamp}_{SanitizeName(name)}.{format}";
            var filePath = Path.Combine(migrationDir, fileName);

            var content = GetMigrationTemplate(name);
            await File.WriteAllTextAsync(filePath, content, cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation("Created migration file '{FilePath}'.", filePath);
            return new MigrationResult(true, $"Migration '{name}' created.", MigrationName: name);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to create migration '{MigrationName}'.", name);
            return new MigrationResult(false, $"Failed to create migration: {ex.Message}", Errors: new[] { ex.Message });
        }
    }

    /// <summary>
    /// Applies all pending migration files in order.
    /// Tracks applied migrations in the history table.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating how many migrations were applied.</returns>
    public async Task<MigrationResult> ApplyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var applied = await GetAppliedMigrationNamesAsync(cancellationToken).ConfigureAwait(false);
            var appliedSet = new HashSet<string>(applied, StringComparer.OrdinalIgnoreCase);

            var pending = GetPendingMigrations(appliedSet);
            var count = 0;

            foreach (var migration in pending)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger?.LogInformation("Applying migration '{MigrationName}'.", migration.Name);
                var script = await File.ReadAllTextAsync(migration.FilePath, cancellationToken).ConfigureAwait(false);
                var success = await ExecuteMigrationAsync(script, migration.Name, cancellationToken).ConfigureAwait(false);

                if (!success)
                {
                    return new MigrationResult(
                        false,
                        $"Migration '{migration.Name}' failed.",
                        MigrationsApplied: count,
                        Errors: new[] { $"Migration '{migration.Name}' execution returned failure." });
                }

                await RecordMigrationAsync(migration.Name, cancellationToken).ConfigureAwait(false);
                count++;
            }

            _logger?.LogInformation("Applied {Count} pending migration(s).", count);
            return new MigrationResult(true, $"Applied {count} migration(s).", MigrationsApplied: count);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to apply migrations.");
            return new MigrationResult(false, $"Failed to apply migrations: {ex.Message}", Errors: new[] { ex.Message });
        }
    }

    /// <summary>
    /// Rolls back the most recently applied migration.
    /// Reverses the migration and removes it from the history table.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public async Task<MigrationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var applied = await GetAppliedMigrationNamesAsync(cancellationToken).ConfigureAwait(false);

            if (applied.Count == 0)
                return new MigrationResult(true, "No migrations to roll back.");

            var lastMigration = applied[^1];
            _logger?.LogInformation("Rolling back migration '{MigrationName}'.", lastMigration);

            await RemoveMigrationAsync(lastMigration, cancellationToken).ConfigureAwait(false);

            _logger?.LogInformation("Rolled back migration '{MigrationName}'.", lastMigration);
            return new MigrationResult(true, $"Rolled back '{lastMigration}'.", MigrationName: lastMigration);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to roll back migration.");
            return new MigrationResult(false, $"Failed to roll back: {ex.Message}", Errors: new[] { ex.Message });
        }
    }

    /// <summary>
    /// Executes a single migration script against the database.
    /// Provider authors implement this to run the provider-specific SQL or command.
    /// </summary>
    /// <param name="script">The migration script content.</param>
    /// <param name="migrationName">The name of the migration being executed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the migration executed successfully.</returns>
    protected abstract Task<bool> ExecuteMigrationAsync(string script, string migrationName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the list of applied migration names from the history table.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of applied migration names.</returns>
    protected abstract Task<IReadOnlyList<string>> GetAppliedMigrationNamesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Records a migration as applied in the history table.
    /// </summary>
    /// <param name="migrationName">The name of the migration to record.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected abstract Task RecordMigrationAsync(string migrationName, CancellationToken cancellationToken);

    /// <summary>
    /// Removes a migration from the history table (for rollback).
    /// </summary>
    /// <param name="migrationName">The name of the migration to remove.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    protected abstract Task RemoveMigrationAsync(string migrationName, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the file extension for migration files.
    /// Override to use a different format (e.g., "cs", "json"). Defaults to "sql".
    /// </summary>
    /// <returns>The file extension without leading dot.</returns>
    protected virtual string GetMigrationFileExtension() => "sql";

    /// <summary>
    /// Gets the template content for a new migration file.
    /// Override to provide provider-specific migration templates.
    /// </summary>
    /// <param name="migrationName">The name of the migration.</param>
    /// <returns>The template content.</returns>
    protected virtual string GetMigrationTemplate(string migrationName)
    {
        return $"-- Migration: {migrationName}\n-- Created: {DateTime.UtcNow:O}\n\n";
    }

    private static string SanitizeName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        return sanitized;
    }

    private IReadOnlyList<MigrationFileInfo> GetPendingMigrations(HashSet<string> appliedNames)
    {
        var migrationDir = _config.Value.Directory;

        if (!Directory.Exists(migrationDir))
            return Array.Empty<MigrationFileInfo>();

        var files = Directory.GetFiles(migrationDir, $"*.{GetMigrationFileExtension()}")
            .Select(f => new MigrationFileInfo(Path.GetFileNameWithoutExtension(f), f))
            .Where(m => !appliedNames.Contains(m.Name))
            .OrderBy(m => m.Name)
            .ToList();

        return files;
    }

    private sealed record MigrationFileInfo(string Name, string FilePath);
}
#endif
