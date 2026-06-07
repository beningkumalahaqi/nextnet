using Microsoft.Extensions.Logging;

namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Performs startup validation of all registered connections.
/// Used when <see cref="MultiDbOptions.ValidateOnStartup"/> is enabled.
/// </summary>
internal sealed class DatabaseSelectorGuard
{
    private readonly ConnectionNameRegistry _nameRegistry;
    private readonly ConnectionPoolRegistry _poolRegistry;
    private readonly ILogger<DatabaseSelectorGuard> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSelectorGuard"/> class.
    /// </summary>
    /// <param name="nameRegistry">The connection name registry.</param>
    /// <param name="poolRegistry">The connection pool registry.</param>
    /// <param name="logger">The logger instance.</param>
    public DatabaseSelectorGuard(
        ConnectionNameRegistry nameRegistry,
        ConnectionPoolRegistry poolRegistry,
        ILogger<DatabaseSelectorGuard> logger)
    {
        _nameRegistry = nameRegistry ?? throw new ArgumentNullException(nameof(nameRegistry));
        _poolRegistry = poolRegistry ?? throw new ArgumentNullException(nameof(poolRegistry));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates that all registered connections have corresponding pool entries
    /// and that their providers are available.
    /// </summary>
    /// <returns>A list of validation errors. Empty if validation passes.</returns>
    public IReadOnlyList<string> Validate()
    {
        var errors = new List<string>();

        foreach (var name in _nameRegistry.Names)
        {
            if (!_nameRegistry.TryGet(name, out var registration))
            {
                errors.Add($"Connection '{name}': registration not found.");
                continue;
            }

            if (!_poolRegistry.TryGet(name, out var poolEntry))
            {
                errors.Add($"Connection '{name}': pool entry not found.");
                continue;
            }

            if (!poolEntry!.IsEnabled)
            {
                _logger.LogWarning("Connection '{ConnectionName}' is disabled and will not be available.", name);
            }

            if (poolEntry.Provider is null)
            {
                errors.Add($"Connection '{name}': provider instance is null.");
            }
        }

        return errors.AsReadOnly();
    }

    /// <summary>
    /// Performs a health probe on all registered connections.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of health check errors. Empty if all connections are healthy.</returns>
    public async Task<IReadOnlyList<string>> ProbeAllAsync(CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        foreach (var name in _nameRegistry.Names)
        {
            if (!_poolRegistry.TryGet(name, out var poolEntry))
            {
                errors.Add($"Connection '{name}': pool entry not found during probe.");
                continue;
            }

            if (!poolEntry!.IsEnabled)
            {
                continue;
            }

            try
            {
                var result = await poolEntry.Provider.IsHealthyAsync(cancellationToken);
                if (!result.IsHealthy)
                {
                    errors.Add($"Connection '{name}': health check failed - {result.Message ?? "Unhealthy"}");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Connection '{name}': health check threw an exception - {ex.Message}");
            }
        }

        return errors.AsReadOnly();
    }
}
