namespace NextNet.Data.HealthChecks.Internal;

/// <summary>
/// In-memory cache for health check results to prevent thundering herds
/// under high traffic or frequent probe requests.
/// </summary>
/// <remarks>
/// <para>
/// Registered as a singleton. Cache entries expire based on the TTL configured
/// in <see cref="NextNetDataHealthCheckOptions.CacheTtl"/>.
/// Thread-safe via <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </para>
/// <para>
/// When a cache miss occurs and a health check is already in progress,
/// concurrent callers will block on the same execution rather than
/// starting redundant checks.
/// </para>
/// </remarks>
public sealed class HealthCheckResultCache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.Ordinal);

    /// <summary>
    /// Attempts to retrieve a cached health check result.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">When this method returns, contains the cached result if found and valid.</param>
    /// <returns><c>true</c> if a valid (non-expired) cached result was found; otherwise, <c>false</c>.</returns>
    public bool TryGet(string key, out HealthCheckResult result)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (!entry.IsExpired)
            {
                result = entry.Result;
                return true;
            }

            // Remove expired entry
            _cache.TryRemove(key, out _);
        }

        result = default;
        return false;
    }

    /// <summary>
    /// Stores a health check result in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="result">The health check result to cache.</param>
    /// <param name="ttl">The time-to-live for this cache entry. If <c>null</c>, defaults to 5 seconds.</param>
    public void Set(string key, HealthCheckResult result, TimeSpan? ttl = null)
    {
        var expiration = DateTime.UtcNow.Add(ttl ?? TimeSpan.FromSeconds(5));
        _cache[key] = new CacheEntry(result, expiration);
    }

    /// <summary>
    /// Invalidates all cached health check results.
    /// Useful for manual refresh or webhook triggers.
    /// </summary>
    public void InvalidateAll()
    {
        _cache.Clear();
    }

    /// <summary>
    /// Gets the number of entries currently in the cache.
    /// </summary>
    public int Count => _cache.Count;

    /// <summary>
    /// Represents a single cached entry with its expiration time.
    /// </summary>
    private sealed record CacheEntry(HealthCheckResult Result, DateTime Expiration)
    {
        /// <summary>
        /// Gets whether this cache entry has expired.
        /// </summary>
        public bool IsExpired => DateTime.UtcNow >= Expiration;
    }
}
