namespace NextNet.Data;

/// <summary>
/// Represents the result of a provider health check operation.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="DataProviderHealthResult"/> record is returned by
/// <see cref="IDataProvider.IsHealthyAsync"/> to communicate the current health
/// state of a data provider. Use the static factory methods <see cref="Healthy"/>
/// and <see cref="Unhealthy"/> to create instances with correct defaults.
/// </para>
/// <example>
/// <code>
/// // Return a healthy result
/// return DataProviderHealthResult.Healthy("All systems operational");
///
/// // Return an unhealthy result with details
/// return DataProviderHealthResult.Unhealthy("Connection refused", innerException);
/// </code>
/// </example>
/// </remarks>
public sealed record DataProviderHealthResult
{
    /// <summary>
    /// Gets whether the provider is healthy.
    /// </summary>
    public bool IsHealthy { get; init; }

    /// <summary>
    /// Gets a human-readable status message describing the health state.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Gets the optional exception that caused the unhealthy state.
    /// Populated when <see cref="IsHealthy"/> is <c>false</c> and an exception was captured.
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Gets the timestamp when this health check was performed.
    /// </summary>
    public DateTimeOffset CheckedAt { get; init; }

    /// <summary>
    /// Gets the duration of the health check operation.
    /// </summary>
    public TimeSpan Duration { get; init; }

    /// <summary>
    /// Creates a healthy health check result.
    /// </summary>
    /// <param name="message">An optional human-readable status message. Defaults to <c>"Healthy"</c>.</param>
    /// <returns>A <see cref="DataProviderHealthResult"/> with <see cref="IsHealthy"/> set to <c>true</c>.</returns>
    public static DataProviderHealthResult Healthy(string? message = null)
        => new()
        {
            IsHealthy = true,
            Message = message ?? "Healthy",
            CheckedAt = DateTimeOffset.UtcNow
        };

    /// <summary>
    /// Creates an unhealthy health check result.
    /// </summary>
    /// <param name="message">A human-readable description of the problem.</param>
    /// <param name="exception">An optional exception that caused the unhealthy state.</param>
    /// <returns>A <see cref="DataProviderHealthResult"/> with <see cref="IsHealthy"/> set to <c>false</c>.</returns>
    public static DataProviderHealthResult Unhealthy(string message, Exception? exception = null)
        => new()
        {
            IsHealthy = false,
            Message = message ?? "Unhealthy",
            Exception = exception,
            CheckedAt = DateTimeOffset.UtcNow
        };
}
