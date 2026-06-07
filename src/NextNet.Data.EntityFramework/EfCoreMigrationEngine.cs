using NextNet.Data.EntityFramework.Internal;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// EF Core implementation of <see cref="IMigrationEngine"/>.
/// Wraps <c>DbContext.Database.Migrate()</c> and <c>dotnet ef migrations</c> CLI calls.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApplyAsync"/> uses <c>DbContext.Database.Migrate()</c> to apply pending migrations.
/// <see cref="AddMigrationAsync"/> shells out to <c>dotnet ef migrations add</c> for migration creation.
/// <see cref="RollbackAsync"/> uses <c>dotnet ef migrations remove</c> to revert the last migration.
/// </para>
/// <para>
/// All operations respect the timeout configured in <see cref="MigrationConfig.TimeoutSeconds"/>.
/// </para>
/// </remarks>
public sealed class EfCoreMigrationEngine : IMigrationEngine
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly MigrationConfig? _config;
    private readonly ILogger<EfCoreMigrationEngine> _logger;
    private readonly MigrationProcessRunner _processRunner;

    /// <summary>
    /// Initializes a new instance of <see cref="EfCoreMigrationEngine"/>.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory for creating context instances.</param>
    /// <param name="config">Optional migration configuration (timeout, directory, history table name).</param>
    /// <param name="logger">Optional logger for migration operations.</param>
    public EfCoreMigrationEngine(
        IDbContextFactory<AppDbContext> contextFactory,
        MigrationConfig? config = null,
        ILogger<EfCoreMigrationEngine>? logger = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _config = config;
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<EfCoreMigrationEngine>();
        _processRunner = new MigrationProcessRunner(_logger);
    }

    /// <summary>
    /// Creates a new migration by running <c>dotnet ef migrations add {name}</c>
    /// in the project directory.
    /// </summary>
    /// <param name="name">A descriptive name for the migration (e.g., "AddUserTable").</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty.</exception>
    public async Task<MigrationResult> AddMigrationAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name), "Migration name must not be null or empty.");
        }

        _logger.LogInformation("Creating migration '{MigrationName}'...", name);

        var migrationDir = _config?.Directory ?? "Migrations";

        try
        {
            var (success, output, error) = await _processRunner.RunProcessAsync(
                "dotnet",
                $"ef migrations add \"{name}\" --context AppDbContext --output-dir \"{migrationDir}\"",
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Migration '{MigrationName}' created successfully.", name);
                return new MigrationResult(
                    true,
                    $"Migration '{name}' created successfully.",
                    MigrationName: name);
            }

            _logger.LogError("Failed to create migration '{MigrationName}': {Error}", name, error);
            return new MigrationResult(
                false,
                $"Failed to create migration '{name}'.",
                Errors: new[] { error ?? "Unknown error" });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Migration creation was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create migration '{MigrationName}'.", name);
            return new MigrationResult(
                false,
                $"Failed to create migration '{name}': {ex.Message}",
                Errors: new[] { ex.ToString() });
        }
    }

    /// <summary>
    /// Applies all pending migrations to the database using <c>DbContext.Database.MigrateAsync()</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> with count of applied migrations.</returns>
    public async Task<MigrationResult> ApplyAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Applying pending migrations...");

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);

            // Apply the configured timeout
            var timeout = _config?.TimeoutSeconds ?? 60;
            context.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeout));

            // Get list of pending migrations before applying
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            var count = pendingMigrations.Count;

            if (count == 0)
            {
                _logger.LogInformation("No pending migrations to apply.");
                return new MigrationResult(true, "No pending migrations to apply.", MigrationsApplied: 0);
            }

            await context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Applied {Count} migration(s) successfully.", count);
            return new MigrationResult(
                true,
                $"Applied {count} migration(s) successfully.",
                MigrationsApplied: count);
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
    /// Rolls back the most recent migration by running <c>dotnet ef migrations remove</c>.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="MigrationResult"/> indicating success or failure.</returns>
    public async Task<MigrationResult> RollbackAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Rolling back the last migration...");

        try
        {
            var (success, output, error) = await _processRunner.RunProcessAsync(
                "dotnet",
                "ef migrations remove --context AppDbContext",
                cancellationToken);

            if (success)
            {
                _logger.LogInformation("Last migration rolled back successfully.");
                return new MigrationResult(
                    true,
                    "Last migration removed successfully.",
                    MigrationsApplied: 1);
            }

            _logger.LogError("Failed to rollback migration: {Error}", error);
            return new MigrationResult(
                false,
                "Failed to rollback migration.",
                Errors: new[] { error ?? "Unknown error" });
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
