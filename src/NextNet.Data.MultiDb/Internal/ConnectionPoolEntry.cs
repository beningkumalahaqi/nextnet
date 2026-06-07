namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Internal entry for a single named connection's pool.
/// Contains the provider instance, optional provider-specific pool object,
/// and health check function.
/// </summary>
/// <param name="ConnectionName">The logical connection name.</param>
/// <param name="ProviderName">The name of the data provider.</param>
/// <param name="ConnectionString">The resolved connection string.</param>
/// <param name="Provider">The <see cref="IDataProvider"/> instance.</param>
/// <param name="Pool">Optional provider-specific pool object (e.g., DbContext pool, Npgsql pool).</param>
/// <param name="HealthCheckFn">Optional function that checks whether the connection is healthy.</param>
/// <param name="IsEnabled">Whether this connection is active.</param>
internal sealed record ConnectionPoolEntry(
    string ConnectionName,
    string ProviderName,
    string ConnectionString,
    NextNet.Data.Abstractions.Abstractions.IDataProvider Provider,
    object? Pool = null,
    Func<CancellationToken, Task<bool>>? HealthCheckFn = null,
    bool IsEnabled = true)
{
    /// <summary>
    /// Gets the creation timestamp. Defaults to <see cref="DateTime.UtcNow"/>.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
