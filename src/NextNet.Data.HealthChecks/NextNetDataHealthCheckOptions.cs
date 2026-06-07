namespace NextNet.Data.HealthChecks;

/// <summary>
/// Configuration options for the NextNet data health check system.
/// Controls endpoint path, response detail level, caching behavior, and error disclosure.
/// </summary>
/// <remarks>
/// <para>
/// These options are bound from the <c>configureOptions</c> delegate passed to
/// <c>AddNextNetHealthChecks</c>.
/// Defaults are designed for production safety — exception details are excluded
/// and caching is enabled to prevent thundering herds on the health endpoint.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetHealthChecks(options =>
/// {
///     options.EndpointPath = "/health/data";
///     options.ShowDetails = true;
///     options.CacheTtl = TimeSpan.FromSeconds(10);
///     options.IncludeExceptionDetails = false;
/// });
/// </code>
/// </example>
/// </remarks>
public sealed class NextNetDataHealthCheckOptions
{
    /// <summary>
    /// Gets or sets the URL path where the health check endpoint is mapped.
    /// Defaults to <c>"/health"</c>.
    /// </summary>
    public string EndpointPath { get; set; } = "/health";

    /// <summary>
    /// Gets or sets whether to include detailed diagnostic data in the health check response.
    /// When <c>false</c>, only overall status and per-provider summary is returned.
    /// Defaults to <c>false</c> for production safety.
    /// </summary>
    public bool ShowDetails { get; set; } = false;

    /// <summary>
    /// Gets or sets the cache duration for health check results.
    /// Consecutive requests within this time window return cached results
    /// instead of executing checks. Set to <see cref="TimeSpan.Zero"/> to disable caching.
    /// Defaults to 5 seconds.
    /// </summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets or sets whether to include exception details (message and stack trace)
    /// in the health check response when a provider check fails.
    /// Defaults to <c>false</c> to avoid leaking internal information.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; } = false;
}
