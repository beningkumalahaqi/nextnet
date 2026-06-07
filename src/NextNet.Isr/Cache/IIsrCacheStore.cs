namespace NextNet.Isr.Cache;

/// <summary>
/// Abstraction for the ISR cache storage layer.
/// Implementations can store pages in memory, on disk, or in a distributed cache (Redis, etc.).
/// </summary>
public interface IIsrCacheStore
{
    /// <summary>
    /// Retrieves a cached page for the specified route.
    /// </summary>
    /// <param name="route">The route path (e.g. <c>"/blog/hello-world"</c>).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>
    /// A <see cref="CachedPage"/> if found; otherwise <c>null</c>.
    /// </returns>
    Task<CachedPage?> GetAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores or updates a cached page for the specified route.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="content">The raw HTML content.</param>
    /// <param name="entry">The cache entry metadata.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetAsync(string route, string content, CacheEntry entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a cached page for the specified route.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><c>true</c> if the entry was removed; <c>false</c> if it did not exist.</returns>
    Task<bool> RemoveAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a cached page exists for the specified route.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><c>true</c> if a cached page exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the cache entry metadata for the specified route without returning the content.
    /// Useful for staleness checks without transferring large HTML payloads.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The <see cref="CacheEntry"/> if found; otherwise <c>null</c>.</returns>
    Task<CacheEntry?> GetMetadataAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds all routes that have at least one of the specified tags.
    /// </summary>
    /// <param name="tags">The tags to search for.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A list of matching route paths.</returns>
    Task<IReadOnlyList<string>> GetRoutesByTagAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a tag mapping for the specified route.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="tag">The tag to add.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddTagAsync(string route, string tag, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tag mapping for the specified route.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="tag">The tag to remove.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveTagAsync(string route, string tag, CancellationToken cancellationToken = default);
}
