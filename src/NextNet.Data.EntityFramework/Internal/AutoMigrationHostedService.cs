using Microsoft.Extensions.Hosting;

namespace NextNet.Data.EntityFramework.Internal;

/// <summary>
/// Hosted service that applies pending EF Core migrations on application startup.
/// Only active when <see cref="EfCoreOptions.AutoApplyMigrations"/> is enabled.
/// </summary>
/// <remarks>
/// <para>
/// This service runs during the application startup phase, before the first request
/// is processed. It ensures the database schema is up to date with the latest migrations.
/// </para>
/// <para>
/// On failure, a warning is logged but the application continues to start
/// (non-fatal). This prevents deployment issues from blocking the entire application.
/// </para>
/// </remarks>
internal sealed class AutoMigrationHostedService : IHostedService
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;
    private readonly EfCoreOptions _options;
    private readonly MigrationConfig? _migrationConfig;
    private readonly ILogger<AutoMigrationHostedService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutoMigrationHostedService"/> class.
    /// </summary>
    /// <param name="contextFactory">The DbContext factory.</param>
    /// <param name="options">The EF Core provider options.</param>
    /// <param name="migrationConfig">Optional migration configuration.</param>
    /// <param name="logger">Optional logger.</param>
    public AutoMigrationHostedService(
        IDbContextFactory<AppDbContext> contextFactory,
        EfCoreOptions options,
        MigrationConfig? migrationConfig = null,
        ILogger<AutoMigrationHostedService>? logger = null)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _migrationConfig = migrationConfig;
        _logger = logger ?? new Microsoft.Extensions.Logging.Abstractions.NullLogger<AutoMigrationHostedService>();
    }

    /// <summary>
    /// Triggered when the application host is ready to start the service.
    /// Applies pending migrations if auto-migration is enabled.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var autoApply = _options.AutoApplyMigrations ?? _migrationConfig?.AutoApply ?? false;

        if (!autoApply)
        {
            _logger.LogInformation("Auto-migration is disabled. Skipping.");
            return;
        }

        _logger.LogInformation("Auto-migration is enabled. Applying pending migrations...");

        try
        {
            await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
            var timeout = _migrationConfig?.TimeoutSeconds ?? 60;
            context.Database.SetCommandTimeout(TimeSpan.FromSeconds(timeout));

            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            var count = pendingMigrations.Count;

            if (count == 0)
            {
                _logger.LogInformation("No pending migrations to apply.");
                return;
            }

            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("Applied {Count} pending migration(s) on startup.", count);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Auto-migration was cancelled on startup.");
            throw;
        }
        catch (Exception ex)
        {
            // Non-fatal: log warning but allow the application to continue
            _logger.LogWarning(ex, "Auto-migration failed on startup: {Message}. " +
                "The application will continue without the latest migrations.", ex.Message);
        }
    }

    /// <summary>
    /// Triggered when the application host is performing a graceful shutdown.
    /// No special shutdown logic is required.
    /// </summary>
    /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
