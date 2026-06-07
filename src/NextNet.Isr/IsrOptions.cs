namespace NextNet.Isr;

/// <summary>
/// Per-route ISR configuration specifying revalidation behaviour,
/// cache tags, and concurrency limits.
/// </summary>
public class IsrOptions
{
    /// <summary>
    /// Gets or sets the revalidation interval in seconds.
    /// After this many seconds have elapsed since the page was generated,
    /// the cached version is considered stale and a background revalidation is triggered.
    /// Set to <c>null</c> to disable time-based revalidation for this route.
    /// </summary>
    public int? Revalidate { get; set; }

    /// <summary>
    /// Gets or sets the cache tags for this route.
    /// Tags enable grouped invalidation — e.g., revalidating all blog posts
    /// by invalidating the <c>"blog"</c> tag.
    /// </summary>
    public string[]? RevalidateTags { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of concurrent regeneration operations
    /// allowed for this route. Prevents cache stampede under high load.
    /// Defaults to <c>1</c>.
    /// </summary>
    public int MaxConcurrentRegenerations { get; set; } = 1;

    /// <summary>
    /// Gets or sets whether stale content may be served while a background
    /// revalidation is in progress. When <c>true</c>, clients always receive
    /// an immediate response (stale if revalidating, fresh otherwise).
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ServeStaleWhileRevalidate { get; set; } = true;

    /// <summary>
    /// Validates that the options are internally consistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    public void Validate()
    {
        if (Revalidate is < 0)
            throw new InvalidOperationException("Revalidate must be a non-negative value or null.");
        if (MaxConcurrentRegenerations < 1)
            throw new InvalidOperationException("MaxConcurrentRegenerations must be at least 1.");
    }
}
