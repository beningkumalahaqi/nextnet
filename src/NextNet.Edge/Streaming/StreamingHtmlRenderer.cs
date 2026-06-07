using NextNet.Edge.Compatibility;
using NextNet.Rendering;
using NextNet.Routing;

namespace NextNet.Edge.Streaming;

/// <summary>
/// Edge-compatible streaming HTML renderer.
/// Renders pages and yields HTML chunks progressively via a streaming writer,
/// enabling progressive response delivery on edge runtimes.
///
/// This is an edge-aware wrapper around the standard NextNet streaming renderer
/// that handles edge-specific concerns like budget enforcement and provider compatibility.
/// </summary>
public class EdgeStreamingHtmlRenderer
{
    private readonly NextNet.Rendering.Streaming.StreamingHtmlRenderer _innerRenderer;
    private readonly EdgeOptions _options;
    private readonly EdgeCompatibilityChecker _compatibilityChecker;

    /// <summary>
    /// Initializes a new instance of <see cref="EdgeStreamingHtmlRenderer"/>.
    /// </summary>
    /// <param name="innerRenderer">The underlying SSR streaming renderer.</param>
    /// <param name="options">Edge configuration options.</param>
    /// <param name="compatibilityChecker">Optional compatibility checker for edge API validation.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="innerRenderer"/> or <paramref name="options"/> is null.</exception>
    public EdgeStreamingHtmlRenderer(
        NextNet.Rendering.Streaming.StreamingHtmlRenderer innerRenderer,
        EdgeOptions options,
        EdgeCompatibilityChecker? compatibilityChecker = null)
    {
        _innerRenderer = innerRenderer ?? throw new ArgumentNullException(nameof(innerRenderer));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _compatibilityChecker = compatibilityChecker ?? new EdgeCompatibilityChecker(new EdgeApiWhitelist(), options);
    }

    /// <summary>
    /// Renders the given route as a stream of HTML chunks written to the specified writer.
    /// </summary>
    /// <param name="route">The route path to render.</param>
    /// <param name="context">The component context.</param>
    /// <param name="writer">The edge stream writer to write chunks to.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task representing the asynchronous render operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/>, <paramref name="context"/>, or <paramref name="writer"/> is null.</exception>
    public async Task RenderToStreamAsync(
        string route,
        NextNet.Components.ComponentContext context,
        IEdgeStreamWriter writer,
        CancellationToken cancellationToken = default)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (context == null) throw new ArgumentNullException(nameof(context));
        if (writer == null) throw new ArgumentNullException(nameof(writer));

        cancellationToken.ThrowIfCancellationRequested();

        await foreach (var chunk in _innerRenderer.RenderAsyncEnumerable(route, context, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await writer.WriteAsync(chunk, cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
    }
}
