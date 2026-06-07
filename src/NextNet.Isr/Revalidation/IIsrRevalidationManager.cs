using NextNet.Isr.Cache;

namespace NextNet.Isr.Revalidation;

/// <summary>
/// Orchestrates ISR revalidation: staleness checks, cache operations,
/// background regeneration, and tag-based invalidation.
/// </summary>
public interface IIsrRevalidationManager
{
    /// <summary>
    /// Determines whether the cached page for the given route is stale.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><c>true</c> if the page is stale or missing; <c>false</c> if fresh.</returns>
    Task<bool> IsStaleAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revalidates a single route by rendering it via SSR and updating the cache.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="RevalidationResult"/> indicating success or failure.</returns>
    Task<RevalidationResult> RevalidateAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all routes that have at least one of the specified tags,
    /// triggering revalidation for each.
    /// </summary>
    /// <param name="tags">The tags to invalidate by.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A <see cref="RevalidationResult"/> with the count of revalidated routes.</returns>
    Task<RevalidationResult> InvalidateByTagsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cached page for the given route, if available.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The cached page, or <c>null</c> if not found.</returns>
    Task<CachedPage?> GetCachedAsync(string route, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stores a page in the cache.
    /// </summary>
    /// <param name="route">The route path.</param>
    /// <param name="content">The HTML content.</param>
    /// <param name="options">The ISR options for this route (revalidation interval, tags, etc.).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetCachedAsync(string route, string content, IsrOptions options, CancellationToken cancellationToken = default);
}
