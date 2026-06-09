namespace NextNet.Isr.Cache;

/// <summary>
/// Metadata stored alongside a cached HTML page, used to determine
/// freshness and support tag-based invalidation.
/// </summary>
public sealed record CacheEntry
{
    /// <summary>
    /// Gets the route path (e.g. <c>"/blog/hello-world"</c>).
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets the UTC timestamp when this page was generated or last revalidated.
    /// </summary>
    public DateTime GeneratedAt { get; }

    /// <summary>
    /// Gets the UTC timestamp after which this page is considered stale.
    /// Derived from <see cref="GeneratedAt"/> + revalidation interval.
    /// </summary>
    public DateTime RevalidateAfter { get; }

    /// <summary>
    /// Gets the cache tags associated with this page.
    /// Used for grouped invalidation (e.g., revalidate all blog posts).
    /// </summary>
    public IReadOnlyList<string> Tags { get; }

    /// <summary>
    /// Gets the content hash (SHA-256) for integrity checking.
    /// </summary>
    public string Hash { get; }

    /// <summary>
    /// Gets the size of the cached content in bytes.
    /// </summary>
    public long Size { get; }

    /// <summary>
    /// Gets the revalidation interval in seconds that was used to compute
    /// <see cref="RevalidateAfter"/>.
    /// </summary>
    public int RevalidateIntervalSeconds { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CacheEntry"/>.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="generatedAt">The UTC generation timestamp.</param>
    /// <param name="revalidateIntervalSeconds">The revalidation interval in seconds.</param>
    /// <param name="tags">Optional cache tags.</param>
    /// <param name="hash">Optional content hash. Computed if not supplied.</param>
    /// <param name="size">The content size in bytes.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="revalidateIntervalSeconds"/> is negative.</exception>
    public CacheEntry(
        string route,
        DateTime generatedAt,
        int revalidateIntervalSeconds,
        IReadOnlyList<string>? tags = null,
        string? hash = null,
        long size = 0)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        GeneratedAt = generatedAt;
        Size = size;

        if (revalidateIntervalSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(revalidateIntervalSeconds), $"[{IsrErrorCodes.RevalidationIntervalMustBeNonNegative}] Revalidation interval must be non-negative.");

        RevalidateIntervalSeconds = revalidateIntervalSeconds;
        RevalidateAfter = revalidateIntervalSeconds > 0
            ? generatedAt.AddSeconds(revalidateIntervalSeconds)
            : DateTime.MaxValue;

        Tags = tags ?? Array.Empty<string>();
        Hash = hash ?? string.Empty;
    }

    /// <summary>
    /// Determines whether this cache entry is stale (past its revalidation time).
    /// </summary>
    /// <param name="now">The current UTC time.</param>
    /// <returns><c>true</c> if the entry is stale; otherwise <c>false</c>.</returns>
    public bool IsStale(DateTime now) => now >= RevalidateAfter;

    /// <summary>
    /// Determines whether this cache entry has the specified tag.
    /// </summary>
    /// <param name="tag">The tag to check.</param>
    /// <returns><c>true</c> if the entry has the tag; otherwise <c>false</c>.</returns>
    public bool HasTag(string tag) => Tags.Contains(tag, StringComparer.OrdinalIgnoreCase);
}
