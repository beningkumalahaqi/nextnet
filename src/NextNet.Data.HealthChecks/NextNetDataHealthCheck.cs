using NextNet.Data.HealthChecks.Internal;

namespace NextNet.Data.HealthChecks;

/// <summary>
/// Aggregates all registered <see cref="IHealthCheckProvider"/> instances
/// into a single ASP.NET Core <see cref="IHealthCheck"/>.
/// Registered automatically by <c>AddNextNetHealthChecks()</c>.
/// </summary>
/// <remarks>
/// <para>
/// This check executes every provider's <see cref="IHealthCheckProvider.GetHealthCheckAsync"/>
/// in parallel using <c>Task.WhenAll</c> and aggregates their results.
/// A provider returning unhealthy status causes the overall check to be unhealthy.
/// Degraded providers (healthy but with warnings) cause a degraded overall status.
/// </para>
/// <para>
/// Provider exceptions are caught per-provider and converted to unhealthy results,
/// preventing a single failing provider from crashing the entire health check.
/// </para>
/// <para>
/// Results are cached for the duration specified in <see cref="NextNetDataHealthCheckOptions.CacheTtl"/>
/// to protect the database from thundering herds under high probe traffic.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Registration is automatic via AddNextNetHealthChecks().
/// // To use the aggregator directly in middleware:
/// var healthCheck = serviceProvider.GetRequiredService&lt;IHealthCheck&gt;();
/// var result = await healthCheck.CheckHealthAsync(context);
/// </code>
/// </example>
public sealed class NextNetDataHealthCheck : IHealthCheck
{
    private readonly IEnumerable<IHealthCheckProvider> _providers;
    private readonly ILogger<NextNetDataHealthCheck> _logger;
    private readonly HealthCheckResultCache _cache;
    private readonly NextNetDataHealthCheckOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetDataHealthCheck"/> class.
    /// </summary>
    /// <param name="providers">The collection of registered health check providers.</param>
    /// <param name="logger">The logger for health check execution.</param>
    /// <param name="cache">The cache for health check results.</param>
    /// <param name="options">The health check configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is <c>null</c>.</exception>
    public NextNetDataHealthCheck(
        IEnumerable<IHealthCheckProvider> providers,
        ILogger<NextNetDataHealthCheck> logger,
        HealthCheckResultCache cache,
        IOptions<NextNetDataHealthCheckOptions> options)
    {
        ArgumentNullException.ThrowIfNull(providers);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(options);

        _providers = providers;
        _logger = logger;
        _cache = cache;
        _options = options.Value;
    }

    /// <summary>
    /// Runs health checks for all registered data providers and returns the aggregated result.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="HealthCheckResult"/> representing the aggregated health status
    /// across all registered providers.
    /// </returns>
    /// <remarks>
    /// <para>
    /// All provider checks are executed in parallel via <c>Task.WhenAll</c>.
    /// If caching is enabled (<see cref="NextNetDataHealthCheckOptions.CacheTtl"/> > <see cref="TimeSpan.Zero"/>),
    /// results are cached and returned to subsequent callers within the TTL window.
    /// </para>
    /// <para>
    /// The aggregation rules are:
    /// <list type="bullet">
    ///   <item>All providers healthy → <see cref="HealthStatus.Healthy"/></item>
    ///   <item>Any provider degraded (but none unhealthy) → <see cref="HealthStatus.Degraded"/></item>
    ///   <item>Any provider unhealthy → <see cref="HealthStatus.Unhealthy"/></item>
    /// </list>
    /// </para>
    /// </remarks>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Check cache first
        const string cacheKey = "nextnet_data_health";
        if (_options.CacheTtl > TimeSpan.Zero && _cache.TryGet(cacheKey, out var cachedResult))
        {
            _logger.LogDebug("Returning cached health check result (TTL: {CacheTtl})", _options.CacheTtl);
            return cachedResult;
        }

        var providerList = _providers.ToList();
        if (providerList.Count == 0)
        {
            _logger.LogWarning("No IHealthCheckProvider implementations registered. Returning Healthy.");
            var emptyResult = HealthCheckResult.Healthy("No data providers registered.");
            return emptyResult;
        }

        _logger.LogInformation("Running health checks for {ProviderCount} provider(s)...", providerList.Count);

        // Execute all provider checks in parallel
        var results = await ExecuteProviderChecksAsync(providerList, cancellationToken);

        // Aggregate results
        var aggregatedResult = AggregateResults(results);

        // Cache the result with configured TTL
        if (_options.CacheTtl > TimeSpan.Zero)
        {
            _cache.Set(cacheKey, aggregatedResult, _options.CacheTtl);
        }

        return aggregatedResult;
    }

    /// <summary>
    /// Executes all provider health checks in parallel and returns their results.
    /// Each provider is wrapped in a try/catch to ensure exception isolation.
    /// </summary>
    private async Task<List<ProviderHealthCheckEntry>> ExecuteProviderChecksAsync(
        List<IHealthCheckProvider> providers,
        CancellationToken cancellationToken)
    {
        var checkTasks = providers.Select(provider => RunSingleProviderCheckAsync(provider, cancellationToken));
        var results = await Task.WhenAll(checkTasks);
        return results.ToList();
    }

    /// <summary>
    /// Runs a single provider's health check, catching any exceptions
    /// so that one provider's failure does not affect others.
    /// </summary>
    private async Task<ProviderHealthCheckEntry> RunSingleProviderCheckAsync(
        IHealthCheckProvider provider,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var providerResult = await provider.GetHealthCheckAsync(cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            var status = MapStatus(providerResult);
            var description = providerResult.Message ?? (providerResult.IsHealthy ? "Healthy" : "Unhealthy");

            var data = new Dictionary<string, object>
            {
                ["provider"] = provider.GetType().Name,
                ["durationMs"] = (long)providerResult.Duration.TotalMilliseconds,
            };

            if (providerResult.Data is not null)
            {
                foreach (var kvp in providerResult.Data)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }

            return new ProviderHealthCheckEntry(
                ProviderName: provider.GetType().Name,
                Status: status,
                Description: description,
                Duration: duration,
                Data: data);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Health check for {ProviderType} was cancelled.", provider.GetType().Name);
            throw;
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex, "Health check for {ProviderType} failed with exception.", provider.GetType().Name);

            var data = new Dictionary<string, object>
            {
                ["provider"] = provider.GetType().Name,
                ["durationMs"] = (long)duration.TotalMilliseconds,
            };

            if (_options.IncludeExceptionDetails)
            {
                data["error"] = ex.Message;
                data["stackTrace"] = ex.ToString();
            }
            else
            {
                data["error"] = "An error occurred during the health check.";
            }

            return new ProviderHealthCheckEntry(
                ProviderName: provider.GetType().Name,
                Status: HealthStatus.Unhealthy,
                Description: ex.Message,
                Duration: duration,
                Data: data);
        }
    }

    /// <summary>
    /// Aggregates individual provider results into a single ASP.NET Core <see cref="HealthCheckResult"/>.
    /// </summary>
    private static HealthCheckResult AggregateResults(List<ProviderHealthCheckEntry> entries)
    {
        var hasUnhealthy = entries.Any(e => e.Status == HealthStatus.Unhealthy);
        var hasDegraded = entries.Any(e => e.Status == HealthStatus.Degraded);

        var totalDuration = entries.Sum(e => e.Duration.TotalMilliseconds);
        var resultData = new Dictionary<string, object>
        {
            ["totalDurationMs"] = (long)totalDuration,
            ["totalProviders"] = entries.Count,
            ["healthyCount"] = entries.Count(e => e.Status == HealthStatus.Healthy),
            ["degradedCount"] = entries.Count(e => e.Status == HealthStatus.Degraded),
            ["unhealthyCount"] = entries.Count(e => e.Status == HealthStatus.Unhealthy),
        };

        var providerDetails = entries.Select(e => new
        {
            name = e.ProviderName,
            status = e.Status.ToString(),
            description = e.Description,
            durationMs = (long)e.Duration.TotalMilliseconds,
            data = e.Data,
        });

        resultData["providers"] = providerDetails;

        if (hasUnhealthy)
        {
            var unhealthyProviders = entries
                .Where(e => e.Status == HealthStatus.Unhealthy)
                .Select(e => e.ProviderName);

            return HealthCheckResult.Unhealthy(
                description: $"Unhealthy provider(s): {string.Join(", ", unhealthyProviders)}",
                data: resultData);
        }

        if (hasDegraded)
        {
            var degradedProviders = entries
                .Where(e => e.Status == HealthStatus.Degraded)
                .Select(e => e.ProviderName);

            return HealthCheckResult.Degraded(
                description: $"Degraded provider(s): {string.Join(", ", degradedProviders)}",
                data: resultData);
        }

        return HealthCheckResult.Healthy(
            description: "All data providers are healthy.",
            data: resultData);
    }

    /// <summary>
    /// Maps the NextNet <see cref="HealthCheckResult"/> (from abstractions) status string
    /// to an ASP.NET Core <see cref="HealthStatus"/> value.
    /// </summary>
    private static HealthStatus MapStatus(Abstractions.Models.HealthCheckResult result)
    {
        if (!result.IsHealthy)
            return HealthStatus.Unhealthy;

        return result.Status?.Equals("Degraded", StringComparison.OrdinalIgnoreCase) == true
            ? HealthStatus.Degraded
            : HealthStatus.Healthy;
    }

    /// <summary>
    /// Internal record representing a single provider's health check execution result.
    /// </summary>
    internal sealed record ProviderHealthCheckEntry(
        string ProviderName,
        HealthStatus Status,
        string Description,
        TimeSpan Duration,
        IReadOnlyDictionary<string, object>? Data);
}
