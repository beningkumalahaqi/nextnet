using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace NextNet.Isr.Cache;

/// <summary>
/// Distributed cache implementation of <see cref="IIsrCacheStore"/> backed by
/// an <see cref="IDistributedCache"/> (e.g. Redis, SQL Server).
/// Serializes cached pages and metadata as JSON.
/// </summary>
public class DistributedIsrCacheStore : IIsrCacheStore
{
    private readonly IDistributedCache _distributedCache;
    private readonly JsonSerializerOptions _jsonOptions;

    private const string ContentPrefix = "isr:content:";
    private const string MetadataPrefix = "isr:meta:";
    private const string TagPrefix = "isr:tag:";

    /// <summary>
    /// Initializes a new instance of <see cref="DistributedIsrCacheStore"/>.
    /// </summary>
    /// <param name="distributedCache">The ASP.NET Core distributed cache.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="distributedCache"/> is null.</exception>
    public DistributedIsrCacheStore(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <inheritdoc />
    public async Task<CachedPage?> GetAsync(string route, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);
        var contentBytes = await _distributedCache.GetAsync(ContentPrefix + normalizedRoute, cancellationToken);
        if (contentBytes == null) return null;

        var metaBytes = await _distributedCache.GetAsync(MetadataPrefix + normalizedRoute, cancellationToken);
        if (metaBytes == null) return null;

        var content = System.Text.Encoding.UTF8.GetString(contentBytes);
        var entry = JsonSerializer.Deserialize<CacheEntry>(metaBytes, _jsonOptions);

        if (entry == null) return null;

        return new CachedPage(route, content, entry);
    }

    /// <inheritdoc />
    public async Task SetAsync(string route, string content, CacheEntry entry, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);
        var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
        var metaBytes = JsonSerializer.SerializeToUtf8Bytes(entry, _jsonOptions);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = entry.RevalidateIntervalSeconds > 0
                ? TimeSpan.FromSeconds(entry.RevalidateIntervalSeconds * 2)
                : null
        };

        await _distributedCache.SetAsync(ContentPrefix + normalizedRoute, contentBytes, options, cancellationToken);
        await _distributedCache.SetAsync(MetadataPrefix + normalizedRoute, metaBytes, options, cancellationToken);

        // Update tag index
        foreach (var tag in entry.Tags)
        {
            await AddTagAsync(route, tag, cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveAsync(string route, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);

        // Retrieve metadata before removing to clean up tags
        var metaBytes = await _distributedCache.GetAsync(MetadataPrefix + normalizedRoute, cancellationToken);
        if (metaBytes != null)
        {
            var entry = JsonSerializer.Deserialize<CacheEntry>(metaBytes, _jsonOptions);
            if (entry != null)
            {
                foreach (var tag in entry.Tags)
                {
                    await RemoveTagAsync(route, tag, cancellationToken);
                }
            }
        }

        await _distributedCache.RemoveAsync(ContentPrefix + normalizedRoute, cancellationToken);
        await _distributedCache.RemoveAsync(MetadataPrefix + normalizedRoute, cancellationToken);

        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string route, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);
        var metaBytes = await _distributedCache.GetAsync(MetadataPrefix + normalizedRoute, cancellationToken);
        return metaBytes != null;
    }

    /// <inheritdoc />
    public async Task<CacheEntry?> GetMetadataAsync(string route, CancellationToken cancellationToken = default)
    {
        var normalizedRoute = NormalizeRoute(route);
        var metaBytes = await _distributedCache.GetAsync(MetadataPrefix + normalizedRoute, cancellationToken);
        if (metaBytes == null) return null;

        return JsonSerializer.Deserialize<CacheEntry>(metaBytes, _jsonOptions);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetRoutesByTagAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var tag in tags)
        {
            var tagBytes = await _distributedCache.GetAsync(TagPrefix + tag, cancellationToken);
            if (tagBytes != null)
            {
                var routes = JsonSerializer.Deserialize<List<string>>(tagBytes, _jsonOptions);
                if (routes != null)
                {
                    foreach (var route in routes)
                    {
                        results.Add(route);
                    }
                }
            }
        }

        return results.ToList();
    }

    /// <inheritdoc />
    public async Task AddTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        var tagKey = TagPrefix + tag;
        var tagBytes = await _distributedCache.GetAsync(tagKey, cancellationToken);
        var routes = tagBytes != null
            ? JsonSerializer.Deserialize<HashSet<string>>(tagBytes, _jsonOptions) ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        routes.Add(NormalizeRoute(route));

        var updatedBytes = JsonSerializer.SerializeToUtf8Bytes(routes, _jsonOptions);
        await _distributedCache.SetAsync(tagKey, updatedBytes, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveTagAsync(string route, string tag, CancellationToken cancellationToken = default)
    {
        var tagKey = TagPrefix + tag;
        var tagBytes = await _distributedCache.GetAsync(tagKey, cancellationToken);
        if (tagBytes == null) return;

        var routes = JsonSerializer.Deserialize<HashSet<string>>(tagBytes, _jsonOptions);
        if (routes == null) return;

        routes.Remove(NormalizeRoute(route));

        if (routes.Count > 0)
        {
            var updatedBytes = JsonSerializer.SerializeToUtf8Bytes(routes, _jsonOptions);
            await _distributedCache.SetAsync(tagKey, updatedBytes, cancellationToken);
        }
        else
        {
            await _distributedCache.RemoveAsync(tagKey, cancellationToken);
        }
    }

    private static string NormalizeRoute(string route)
    {
        var normalized = route.StartsWith('/') ? route : "/" + route;
        return normalized.TrimEnd('/').ToLowerInvariant();
    }
}
