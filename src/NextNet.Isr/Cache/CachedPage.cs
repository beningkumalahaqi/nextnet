namespace NextNet.Isr.Cache;

/// <summary>
/// Represents a cached HTML page with its content and associated metadata.
/// Returned by <see cref="IIsrCacheStore"/> when a cache entry is found.
/// </summary>
public class CachedPage
{
    /// <summary>
    /// Gets the route path for this cached page (e.g. <c>"/blog/hello-world"</c>).
    /// </summary>
    public string Route { get; }

    /// <summary>
    /// Gets the raw HTML content of the page.
    /// </summary>
    public string Content { get; }

    /// <summary>
    /// Gets the cache entry metadata associated with this page.
    /// </summary>
    public CacheEntry Metadata { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CachedPage"/>.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="content">The raw HTML content.</param>
    /// <param name="metadata">The cache entry metadata.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public CachedPage(string route, string content, CacheEntry metadata)
    {
        Route = route ?? throw new ArgumentNullException(nameof(route));
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
    }
}
