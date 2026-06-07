using System.Collections.Concurrent;

namespace NextNet.Isr.Cache;

/// <summary>
/// In-memory implementation of <see cref="IIsrCacheStore"/>.
/// Stores cached pages and tag mappings in concurrent dictionaries.
/// Suitable for single-server deployments and testing.
/// </summary>
public class MemoryIsrCacheStore : IIsrCacheStore
{
    private readonly ConcurrentDictionary<string, CachedPage> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, HashSet<string>> _tagIndex = new(StringComparer.OrdinalIgnoreCase);
    private readonly object _lock = new();

    /// <inheritdoc />
    public Task<CachedPage?> GetAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(route, out var page))
            return Task.FromResult<CachedPage?>(page);

        return Task.FromResult<CachedPage?>(null);
    }

    /// <inheritdoc />
    public Task SetAsync(string route, string content, CacheEntry entry, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var page = new CachedPage(route, content, entry);
        _cache[route] = page;

        // Update tag index
        foreach (var tag in entry.Tags)
        {
            var routes = _tagIndex.GetOrAdd(tag, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
            lock (_lock)
            {
                routes.Add(route);
            }
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> RemoveAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!_cache.TryRemove(route, out var removed))
            return Task.FromResult(false);

        // Clean up tag index
        foreach (var tag in removed.Metadata.Tags)
        {
            if (_tagIndex.TryGetValue(tag, out var routes))
            {
                lock (_lock)
                {
                    routes.Remove(route);
                }
            }
        }

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> ExistsAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(_cache.ContainsKey(route));
    }

    /// <inheritdoc />
    public Task<CacheEntry?> GetMetadataAsync(string route, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(route, out var page))
            return Task.FromResult<CacheEntry?>(page.Metadata);

        return Task.FromResult<CacheEntry?>(null);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<string>> GetRoutesByTagAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            if (_tagIndex.TryGetValue(tag, out var routes))
            {
                lock (_lock)
                {
                    foreach (var route in routes)
                    {
                        results.Add(route);
                    }
                }
            }
        }

        return Task.FromResult<IReadOnlyList<string>>(results.ToList());
    }

    /// <inheritdoc />
    public Task AddTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var routes = _tagIndex.GetOrAdd(tag, _ => new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        lock (_lock)
        {
            routes.Add(route);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_tagIndex.TryGetValue(tag, out var routes))
        {
            lock (_lock)
            {
                routes.Remove(route);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Removes all cached pages and tag mappings. Used for testing.
    /// </summary>
    public void Clear()
    {
        _cache.Clear();
        _tagIndex.Clear();
    }

    /// <summary>
    /// Gets the total number of cached pages.
    /// </summary>
    public int Count => _cache.Count;
}
