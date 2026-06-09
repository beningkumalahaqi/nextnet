using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using NextNet.Components;
using NextNet.Isr.Cache;
using NextNet.Logging;
using NextNet.Rendering;

// Note: CryptographicOperations is used by OnDemandRevalidator via the same using.

namespace NextNet.Isr.Revalidation;

/// <summary>
/// Default implementation of <see cref="IIsrRevalidationManager"/>.
/// Coordinates between the cache store, SSR renderer, and revalidation queue
/// to serve fresh or stale content while regenerating pages in the background.
/// </summary>
public sealed class IsrRevalidationManager : IIsrRevalidationManager
{
    private readonly IIsrCacheStore _cacheStore;
    private readonly SsrRenderer _ssrRenderer;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IsrGlobalOptions _globalOptions;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="IsrRevalidationManager"/>.
    /// </summary>
    /// <param name="cacheStore">The ISR cache store.</param>
    /// <param name="ssrRenderer">The SSR renderer for regeneration.</param>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="globalOptions">Global ISR options.</param>
    /// <param name="logger">Optional logger.</param>
    public IsrRevalidationManager(
        IIsrCacheStore cacheStore,
        SsrRenderer ssrRenderer,
        IHttpContextAccessor httpContextAccessor,
        IsrGlobalOptions globalOptions,
        INextNetLogger? logger = null)
    {
        _cacheStore = cacheStore ?? throw new ArgumentNullException(nameof(cacheStore));
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _globalOptions = globalOptions ?? throw new ArgumentNullException(nameof(globalOptions));
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> IsStaleAsync(string route, CancellationToken cancellationToken = default)
    {
        var metadata = await _cacheStore.GetMetadataAsync(route, cancellationToken);

        if (metadata == null)
            return true; // Missing page is considered stale

        return metadata.IsStale(DateTime.UtcNow);
    }

    /// <inheritdoc />
    public async Task<RevalidationResult> RevalidateAsync(string route, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger?.Info("Revalidating route {Route}", route);

            // Render the page via SSR
            var context = new ComponentContext(_httpContextAccessor.HttpContext ?? new DefaultHttpContext());
            var htmlResponse = await _ssrRenderer.RenderAsync(route, context, cancellationToken);

            var content = htmlResponse.Content.ToHtml();

            // Compute content hash
            var hash = ComputeHash(content);

            // Detect the revalidation interval from route metadata (default to global)
            var revalidateSeconds = _globalOptions.DefaultRevalidateSeconds;

            var entry = new CacheEntry(
                route: route,
                generatedAt: DateTime.UtcNow,
                revalidateIntervalSeconds: revalidateSeconds,
                hash: hash,
                size: Encoding.UTF8.GetByteCount(content));

            await _cacheStore.SetAsync(route, content, entry, cancellationToken);

            _logger?.Info("Successfully revalidated route {Route} ({Size} bytes)", route, entry.Size);
            return RevalidationResult.Ok(route);
        }
        catch (OperationCanceledException)
        {
            _logger?.Warn("Revalidation cancelled for route {Route}", route);
            return RevalidationResult.Fail($"[{IsrErrorCodes.RevalidationCancelled}] Revalidation cancelled for {route}");
        }
        catch (Exception ex)
        {
            _logger?.Error("Failed to revalidate route {Route}: {Exception}", route, ex);
            return RevalidationResult.Fail($"[{IsrErrorCodes.RevalidationFailedForRoute}] Failed to revalidate {route}: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<RevalidationResult> InvalidateByTagsAsync(IReadOnlyList<string> tags, CancellationToken cancellationToken = default)
    {
        if (tags == null || tags.Count == 0)
            return RevalidationResult.Fail($"[{IsrErrorCodes.NoTagsProvidedForInvalidation}] No tags provided for invalidation.");

        try
        {
            _logger?.Info("Invalidating routes by tags: {Tags}", string.Join(", ", tags));

            var routes = await _cacheStore.GetRoutesByTagAsync(tags, cancellationToken);
            if (routes.Count == 0)
            {
                _logger?.Warn("No routes found for tags: {Tags}", string.Join(", ", tags));
                return RevalidationResult.Ok(Array.Empty<string>());
            }

            var revalidatedRoutes = new List<string>();
            foreach (var route in routes)
            {
                var result = await RevalidateAsync(route, cancellationToken);
                if (result.Success)
                {
                    revalidatedRoutes.Add(route);
                }
            }

            _logger?.Info("Tag-based invalidation complete: {Count} routes revalidated", revalidatedRoutes.Count);
            return RevalidationResult.Ok(revalidatedRoutes);
        }
        catch (Exception ex)
        {
            _logger?.Error("Tag-based invalidation failed: {Exception}", ex);
            return RevalidationResult.Fail($"[{IsrErrorCodes.TagBasedInvalidationFailed}] Tag-based invalidation failed: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public Task<CachedPage?> GetCachedAsync(string route, CancellationToken cancellationToken = default)
    {
        return _cacheStore.GetAsync(route, cancellationToken);
    }

    /// <inheritdoc />
    public async Task SetCachedAsync(string route, string content, IsrOptions options, CancellationToken cancellationToken = default)
    {
        var hash = ComputeHash(content);
        var revalidateSeconds = options.Revalidate ?? _globalOptions.DefaultRevalidateSeconds;

        var entry = new CacheEntry(
            route: route,
            generatedAt: DateTime.UtcNow,
            revalidateIntervalSeconds: revalidateSeconds,
            tags: options.RevalidateTags,
            hash: hash,
            size: Encoding.UTF8.GetByteCount(content));

        await _cacheStore.SetAsync(route, content, entry, cancellationToken);
    }

    /// <summary>
    /// Computes a SHA-256 hash of the content for integrity checking.
    /// </summary>
    internal static string ComputeHash(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(content);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}
