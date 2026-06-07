using System.Text;
using Microsoft.AspNetCore.Http;
using NextNet.Components;
using NextNet.Isr.Background;
using NextNet.Isr.Cache;
using NextNet.Isr.Manifest;
using NextNet.Isr.Revalidation;
using NextNet.Logging;
using NextNet.Rendering;

namespace NextNet.Isr.Middleware;

/// <summary>
/// ASP.NET Core middleware that implements the stale-while-revalidate pattern.
/// For ISR-configured routes:
/// - Cache HIT + fresh → serve cached HTML
/// - Cache HIT + stale → serve stale HTML + enqueue background revalidation
/// - Cache MISS → render via SSR → cache → serve
/// Routes without ISR configuration pass through to the next middleware.
/// </summary>
public class IsrMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IIsrCacheStore _cacheStore;
    private readonly SsrRenderer _ssrRenderer;
    private readonly IsrManifest _isrManifest;
    private readonly RevalidationQueue _revalidationQueue;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="IsrMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="cacheStore">The ISR cache store.</param>
    /// <param name="ssrRenderer">The SSR renderer.</param>
    /// <param name="isrManifest">The ISR manifest with per-route configuration.</param>
    /// <param name="revalidationQueue">The revalidation queue for background regeneration.</param>
    /// <param name="logger">Optional logger.</param>
    public IsrMiddleware(
        RequestDelegate next,
        IIsrCacheStore cacheStore,
        SsrRenderer ssrRenderer,
        IsrManifest isrManifest,
        RevalidationQueue revalidationQueue,
        INextNetLogger? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _cacheStore = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _isrManifest = isrManifest ?? throw new ArgumentNullException(nameof(isrManifest));
        _revalidationQueue = revalidationQueue ?? throw new ArgumentNullException(nameof(revalidationQueue));
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware for the given HTTP context.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var request = context.Request;

        // Only handle GET and HEAD requests
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            await _next(context);
            return;
        }

        // Skip non-HTML requests
        var acceptHeader = request.Headers.Accept.ToString();
        if (!string.IsNullOrEmpty(acceptHeader) &&
            !acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase) &&
            !acceptHeader.Contains("*/*", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Skip if ISR is not configured (no routes)
        if (!_isrManifest.HasIsrRoutes)
        {
            await _next(context);
            return;
        }

        var route = request.Path.Value ?? "/";

        // Check if this route has ISR configuration
        // We still check the cache for non-ISR routes if they happen to be cached
        var isIsrRoute = _isrManifest.TryGetMetadata(route, out var routeMetadata);

        // Try to get from cache
        var cached = await _cacheStore.GetAsync(route, context.RequestAborted);

        if (cached != null)
        {
            // Cache HIT
            var isStale = cached.Metadata.IsStale(DateTime.UtcNow);

            if (!isStale)
            {
                // Cache HIT + FRESH → serve immediately
                _logger?.Debug("ISR cache HIT (fresh) for {Route}", route);
                await ServeCachedResponse(context, cached);
                return;
            }

            // Cache HIT + STALE
            if (isIsrRoute && routeMetadata?.ServeStaleWhileRevalidate != false)
            {
                // Serve stale content and trigger background revalidation
                _logger?.Info("ISR cache HIT (stale) for {Route} — serving stale + background revalidation", route);
                await ServeCachedResponse(context, cached);

                // Enqueue background revalidation (fire-and-forget)
                var revalidationRequest = new RevalidationRequest
                {
                    Route = route,
                    Reason = $"Stale-while-revalidate: age exceeded interval"
                };
                await _revalidationQueue.EnqueueAsync(revalidationRequest, context.RequestAborted);

                return;
            }

            // Stale but serve-stale is disabled — fall through to SSR render
            _logger?.Debug("ISR cache STALE for {Route} — serve-stale disabled, re-rendering", route);
        }

        // Cache MISS — render via SSR (or fall through to next middleware if not an ISR route)
        if (!isIsrRoute)
        {
            await _next(context);
            return;
        }

        _logger?.Info("ISR cache MISS for {Route} — rendering via SSR", route);

        try
        {
            var context2 = new ComponentContext(context);
            var htmlResponse = await _ssrRenderer.RenderAsync(route, context2, context.RequestAborted);
            var html = htmlResponse.Content.ToHtml();

            // Cache the rendered page
            var options = routeMetadata?.ToOptions() ?? new IsrOptions();
            var hash = IsrRevalidationManager.ComputeHash(html);
            var entry = new CacheEntry(
                route: route,
                generatedAt: DateTime.UtcNow,
                revalidateIntervalSeconds: options.Revalidate ?? _isrManifest.GlobalOptions.DefaultRevalidateSeconds,
                tags: options.RevalidateTags,
                hash: hash,
                size: Encoding.UTF8.GetByteCount(html));

            await _cacheStore.SetAsync(route, html, entry, context.RequestAborted);

            // Serve the fresh content
            await htmlResponse.ExecuteAsync(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no-op
        }
        catch (Exception ex)
        {
            _logger?.Error("ISR SSR render failed for {Route}: {Exception}", route, ex);
            // Fall through to next middleware (or error handler)
            await _next(context);
        }
    }

    private static async Task ServeCachedResponse(HttpContext context, CachedPage cached)
    {
        var response = context.Response;
        response.StatusCode = StatusCodes.Status200OK;
        response.ContentType = "text/html; charset=utf-8";

        // Add a header to indicate this is an ISR-served response
        response.Headers["X-NextNet-ISR"] = "hit";
        response.Headers["X-NextNet-ISR-Generated"] = cached.Metadata.GeneratedAt.ToString("O");

        var bytes = Encoding.UTF8.GetBytes(cached.Content);
        await response.Body.WriteAsync(bytes, context.RequestAborted);
    }
}
