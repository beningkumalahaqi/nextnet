using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Logging;
using NextNet.Routing;

namespace NextNet.Rendering.Streaming;

/// <summary>
/// Provides streaming SSR capabilities. Renders pages and yields HTML chunks
/// via <see cref="IAsyncEnumerable{T}"/>, enabling progressive response delivery.
///
/// Instead of waiting for the full page to render, this renderer yields content
/// progressively: outermost layout shell (head, header) first, then the full
/// page content (with inner layouts applied via <see cref="LayoutRenderer"/>),
/// then the outermost layout footer/scripts. This enables the browser to begin
/// processing the document shell while the page content is still being generated.
///
/// Streams at the outermost layout level for maximum compatibility. Inner layouts
/// that do not implement <see cref="ILayout.RenderShell"/> / <see cref="ILayout.RenderFooter"/>
/// are handled transparently via the standard layout rendering pipeline.
/// </summary>
/// <example>
/// <code>
/// // Use with SsrMiddleware for progressive rendering:
/// await foreach (var chunk in streamingRenderer.RenderAsyncEnumerable(
///     "/about", componentContext, cancellationToken))
/// {
///     var bytes = Encoding.UTF8.GetBytes(chunk);
///     await response.Body.WriteAsync(bytes, cancellationToken);
///     await response.Body.FlushAsync(cancellationToken);
/// }
/// </code>
/// </example>
public sealed class StreamingHtmlRenderer
{
    private readonly SsrRenderer _ssrRenderer;
    private readonly SsrOptions _options;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="StreamingHtmlRenderer"/>.
    /// </summary>
    /// <param name="ssrRenderer">The underlying SSR renderer.</param>
    /// <param name="options">SSR options controlling buffer size and streaming behaviour.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="ssrRenderer"/> is null.</exception>
    public StreamingHtmlRenderer(
        SsrRenderer ssrRenderer,
        SsrOptions? options = null,
        INextNetLogger? logger = null)
    {
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _options = options ?? ssrRenderer.Options;
        _logger = logger;
    }

    /// <summary>
    /// Renders the given route as a stream of HTML chunks.
    /// Yields progressively: outermost layout shell -> full page content (with inner layouts) -> outermost layout footer.
    /// </summary>
    /// <param name="route">The route path to render.</param>
    /// <param name="context">The component context.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An async enumerable of HTML string chunks.</returns>
    public async IAsyncEnumerable<string> RenderAsyncEnumerable(
        string route,
        ComponentContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Resolve route
        RouteEntry? entry;
        bool resolveFailed = false;
        try
        {
            entry = _ssrRenderer.ResolveRoute(route);
        }
        catch (Exception ex)
        {
            _logger?.Error("Failed to resolve route {Route}: {Exception}", route, ex);
            entry = null;
            resolveFailed = true;
        }

        if (resolveFailed)
        {
            yield return HtmlResponse.NotFound().ToString();
            yield break;
        }

        if (entry == null)
        {
            _logger?.Debug("Route {Route} not found for streaming — yielding 404", route);
            yield return HtmlResponse.NotFound().ToString();
            yield break;
        }

        _logger?.Info("Streaming route {Route} (layout chain length: {ChainLength})",
            route, entry.LayoutChain.Count);

        cancellationToken.ThrowIfCancellationRequested();

        // Iterate through the inner producer; error handling outside yield
        var innerStream = ProduceStreamInternal(entry, context, cancellationToken);
        await using var enumerator = innerStream.GetAsyncEnumerator(cancellationToken);

        HtmlResponse? errorResponse = null;

        while (true)
        {
            bool hasNext;
            try
            {
                hasNext = await enumerator.MoveNextAsync();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger?.Error("Failed during streaming for route {Route}: {Exception}", route, ex);
                errorResponse = await _ssrRenderer.RenderErrorAsync(ex);
                break;
            }

            if (!hasNext) break;
            yield return enumerator.Current;
        }

        if (errorResponse != null)
        {
            yield return errorResponse.ToString();
        }
    }

    /// <summary>
    /// Produces the progressive stream of HTML chunks.
    /// Step 1: Outermost layout shell (if the layout supports it)
    /// Step 2: Inner content (page wrapped by inner layouts only, excluding the outermost)
    /// Step 3: Outermost layout footer (if the layout supports it)
    ///
    /// If the outermost layout does not support streaming (default shell/footer return empty),
    /// falls back to rendering the full content with all layouts.
    /// </summary>
    private async IAsyncEnumerable<string> ProduceStreamInternal(
        RouteEntry entry,
        ComponentContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var layoutChain = entry.LayoutChain;
        var sp = _ssrRenderer.ServiceProvider;

        // ── Step 1: Outermost layout shell ──────────────────────────────────
        string? outermostShell = null;
        string? outermostFooter = null;

        if (layoutChain.Count > 0)
        {
            var outermostPath = layoutChain[layoutChain.Count - 1];
            var outermostLayout = ResolveLayoutInstance(outermostPath);
            if (outermostLayout != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var shell = await outermostLayout.RenderShell();
                var footer = await outermostLayout.RenderFooter();
                outermostShell = shell.ToHtml();
                outermostFooter = footer.ToHtml();
            }
        }

        // Determine if the outermost layout supports streaming (has non-empty shell/footer)
        var outermostSupportsStreaming = !string.IsNullOrEmpty(outermostShell) ||
                                         !string.IsNullOrEmpty(outermostFooter);

        if (outermostSupportsStreaming && !string.IsNullOrEmpty(outermostShell))
        {
            foreach (var chunk in ChunkString(outermostShell, _options.BufferSize))
            {
                yield return chunk;
            }
        }

        // ── Step 2: Inner content with layouts applied ──────────────────────
        _logger?.Debug("Rendering page content for {Route}", entry.RoutePattern);
        var pageContent = await _ssrRenderer.RenderPageAsync(entry, context, cancellationToken);

        IHtmlContent innerContent;
        if (outermostSupportsStreaming && layoutChain.Count > 1)
        {
            // Apply only inner layouts (all except the outermost)
            var innerLayouts = new List<string>(layoutChain);
            innerLayouts.RemoveAt(layoutChain.Count - 1);
            innerContent = await _ssrRenderer.LayoutRendererInstance.RenderAsync(
                pageContent, innerLayouts, sp);
        }
        else if (outermostSupportsStreaming)
        {
            // Only one layout and it supports streaming — just render the page
            innerContent = pageContent;
        }
        else
        {
            // Outermost layout doesn't support streaming — render everything normally
            innerContent = await _ssrRenderer.LayoutRendererInstance.RenderAsync(
                pageContent, layoutChain, sp);
        }

        var contentHtml = innerContent.ToHtml();
        foreach (var chunk in ChunkString(contentHtml, _options.BufferSize))
        {
            yield return chunk;
        }

        // ── Step 3: Outermost layout footer ─────────────────────────────────
        if (outermostSupportsStreaming && !string.IsNullOrEmpty(outermostFooter))
        {
            foreach (var chunk in ChunkString(outermostFooter, _options.BufferSize))
            {
                yield return chunk;
            }
        }
    }

    /// <summary>
    /// Resolves a layout instance from the given layout path using DI.
    /// </summary>
    private ILayout? ResolveLayoutInstance(string layoutPath)
    {
        var layoutType = _ssrRenderer.ComponentResolver.GetLayoutType(layoutPath);
        if (layoutType == null)
        {
            _logger?.Warn("Cannot resolve layout type for {LayoutPath}", layoutPath);
            return null;
        }

        try
        {
            return (ILayout)_ssrRenderer.ServiceProvider.GetRequiredService(layoutType);
        }
        catch (InvalidOperationException ex)
        {
            _logger?.Warn("Layout type {LayoutType} not registered in DI: {Exception}", layoutType.FullName, ex);
            return null;
        }
    }

    /// <summary>
    /// Splits a string into chunks of the specified maximum size.
    /// </summary>
    private static IEnumerable<string> ChunkString(string value, int chunkSize)
    {
        if (string.IsNullOrEmpty(value))
            yield break;

        if (chunkSize <= 0)
            chunkSize = 8192;

        for (int i = 0; i < value.Length; i += chunkSize)
        {
            var len = Math.Min(chunkSize, value.Length - i);
            yield return value.Substring(i, len);
        }
    }
}
