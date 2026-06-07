namespace NextNet.Isr;

/// <summary>
/// Global ISR configuration options that apply as defaults to all ISR-configured routes.
/// Can be overridden per-route via <see cref="IsrOptions"/> or <see cref="Manifest.IsrRouteAttribute"/>.
/// </summary>
public class IsrGlobalOptions
{
    /// <summary>
    /// Gets or sets the default revalidation interval in seconds.
    /// Routes without explicit ISR configuration use this value.
    /// Defaults to <c>60</c>.
    /// </summary>
    public int DefaultRevalidateSeconds { get; set; } = 60;

    /// <summary>
    /// Gets or sets the maximum number of concurrent regeneration operations
    /// allowed globally. Prevents resource exhaustion under high load.
    /// Defaults to <c>4</c>.
    /// </summary>
    public int MaxConcurrentRegenerations { get; set; } = 4;

    /// <summary>
    /// Gets or sets the maximum number of pending revalidation requests in the queue.
    /// When exceeded, the oldest requests are dropped.
    /// Defaults to <c>100</c>.
    /// </summary>
    public int MaxPendingRevalidations { get; set; } = 100;

    /// <summary>
    /// Gets or sets the deduplication window in seconds.
    /// Within this window, duplicate revalidation requests for the same route
    /// are silently dropped.
    /// Defaults to <c>30</c> seconds.
    /// </summary>
    public int DeduplicationWindowSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the secret required for on-demand revalidation API calls.
    /// When <c>null</c> or empty, no authentication is required.
    /// </summary>
    public string? RevalidationSecret { get; set; }

    /// <summary>
    /// Gets or sets the secret used for webhook HMAC-SHA256 signature verification.
    /// When <c>null</c> or empty, no signature verification is performed.
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Gets or sets whether stale content may be served while a background
    /// revalidation is in progress. This is the global default; can be
    /// overridden per-route.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ServeStaleWhileRevalidate { get; set; } = true;

    /// <summary>
    /// Validates the global options for consistency.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (DefaultRevalidateSeconds < 0)
            throw new InvalidOperationException("DefaultRevalidateSeconds must be non-negative.");
        if (MaxConcurrentRegenerations < 1)
            throw new InvalidOperationException("MaxConcurrentRegenerations must be at least 1.");
        if (MaxPendingRevalidations < 1)
            throw new InvalidOperationException("MaxPendingRevalidations must be at least 1.");
        if (DeduplicationWindowSeconds < 0)
            throw new InvalidOperationException("DeduplicationWindowSeconds must be non-negative.");
    }
}
