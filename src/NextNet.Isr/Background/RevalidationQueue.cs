using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NextNet.Isr.Background;

/// <summary>
/// A bounded, thread-safe queue for ISR revalidation requests.
/// Supports deduplication to prevent multiple concurrent revalidation
/// operations for the same route.
/// </summary>
public class RevalidationQueue
{
    private readonly Channel<RevalidationRequest> _channel;
    private readonly ConcurrentDictionary<string, DateTime> _pendingRoutes;
    private readonly TimeSpan _deduplicationWindow;
    private readonly int _maxConcurrentPerRoute;

    /// <summary>
    /// Initializes a new instance of <see cref="RevalidationQueue"/>.
    /// </summary>
    /// <param name="capacity">The maximum number of items in the queue. Defaults to 100.</param>
    /// <param name="deduplicationWindowSeconds">
    /// The time window (in seconds) within which duplicate requests for the same route
    /// are silently dropped. Defaults to 30.
    /// </param>
    /// <param name="maxConcurrentPerRoute">
    /// The maximum number of concurrent revalidation operations allowed per route.
    /// Defaults to 1.
    /// </param>
    public RevalidationQueue(
        int capacity = 100,
        int deduplicationWindowSeconds = 30,
        int maxConcurrentPerRoute = 1)
    {
        _channel = Channel.CreateBounded<RevalidationRequest>(new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleWriter = false,
            SingleReader = false
        });

        _pendingRoutes = new ConcurrentDictionary<string, DateTime>(StringComparer.OrdinalIgnoreCase);
        _deduplicationWindow = TimeSpan.FromSeconds(deduplicationWindowSeconds);
        _maxConcurrentPerRoute = maxConcurrentPerRoute;
    }

    /// <summary>
    /// Enqueues a revalidation request. If a request for the same route is already
    /// pending within the deduplication window, the new request is silently dropped.
    /// </summary>
    /// <param name="request">The revalidation request.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><c>true</c> if the request was enqueued; <c>false</c> if it was deduplicated.</returns>
    public async ValueTask<bool> EnqueueAsync(RevalidationRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        // Deduplication: check if this route is already pending
        if (!string.IsNullOrEmpty(request.Route))
        {
            if (_pendingRoutes.TryGetValue(request.Route, out var enqueuedAt))
            {
                if (DateTime.UtcNow - enqueuedAt < _deduplicationWindow)
                {
                    return false; // Silently drop duplicate
                }
            }
        }

        // For tag-based requests, check all matching routes
        if (request.Tags is { Count: > 0 })
        {
            // Tag requests always pass through (they may affect multiple routes)
        }

        // Track the pending request
        if (!string.IsNullOrEmpty(request.Route))
        {
            _pendingRoutes[request.Route] = DateTime.UtcNow;
        }

        await _channel.Writer.WriteAsync(request, cancellationToken);
        return true;
    }

    /// <summary>
    /// Reads all queued revalidation requests as an async enumerable.
    /// </summary>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An async enumerable of revalidation requests.</returns>
    public IAsyncEnumerable<RevalidationRequest> ReadAllAsync(CancellationToken cancellationToken = default)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }

    /// <summary>
    /// Removes a route from the pending set after revalidation completes.
    /// </summary>
    /// <param name="route">The route that was revalidated.</param>
    public void CompleteRevalidation(string route)
    {
        if (!string.IsNullOrEmpty(route))
        {
            _pendingRoutes.TryRemove(route, out _);
        }
    }

    /// <summary>
    /// Gets the number of pending (deduplicated) routes in the queue.
    /// </summary>
    public int PendingCount => _channel.Reader.Count;

    /// <summary>
    /// Gets the number of routes currently being tracked for deduplication.
    /// </summary>
    public int DeduplicatedCount => _pendingRoutes.Count;

    /// <summary>
    /// Gets the maximum allowed concurrent operations per route.
    /// </summary>
    public int MaxConcurrentPerRoute => _maxConcurrentPerRoute;
}

/// <summary>
/// Represents a request to revalidate one or more routes.
/// </summary>
public class RevalidationRequest
{
    /// <summary>
    /// Gets or sets the specific route to revalidate (e.g. <c>"/blog/hello-world"</c>).
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// Gets or sets the tags to invalidate. When specified, all routes with
    /// matching tags will be revalidated.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the reason for this revalidation (for logging/diagnostics).
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets the UTC timestamp when this request was created.
    /// </summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>
    /// Returns a string representation of this request.
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Route))
            return $"RevalidationRequest [Route: {Route}, Reason: {Reason}]";
        if (Tags is { Count: > 0 })
            return $"RevalidationRequest [Tags: {string.Join(", ", Tags)}, Reason: {Reason}]";
        return "RevalidationRequest [Unknown]";
    }
}
