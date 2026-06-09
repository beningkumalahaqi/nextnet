using System.Text;
using Microsoft.AspNetCore.Http;
using NextNet.Components;
using NextNet.Logging;
using NextNet.Rendering.Streaming;

namespace NextNet.Rendering.Middleware;

/// <summary>
/// ASP.NET Core middleware that intercepts HTTP requests and renders NextNet pages
/// using SSR. Falls through to the next middleware when no matching route is found.
/// Supports both standard and streaming rendering modes.
/// </summary>
public sealed class SsrMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SsrRenderer _ssrRenderer;
    private readonly StreamingHtmlRenderer _streamingRenderer;
    private readonly SsrOptions _options;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="SsrMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="ssrRenderer">The SSR renderer.</param>
    /// <param name="streamingRenderer">The streaming renderer.</param>
    /// <param name="options">SSR options.</param>
    /// <param name="logger">Optional logger.</param>
    public SsrMiddleware(
        RequestDelegate next,
        SsrRenderer ssrRenderer,
        StreamingHtmlRenderer? streamingRenderer = null,
        SsrOptions? options = null,
        INextNetLogger? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _streamingRenderer = streamingRenderer ?? new StreamingHtmlRenderer(ssrRenderer, options, logger);
        _options = options ?? ssrRenderer.Options;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware for the given HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        if (context == null) throw new ArgumentNullException(nameof(context));

        var request = context.Request;

        // Only handle GET and HEAD requests for page rendering
        if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
        {
            await _next(context);
            return;
        }

        // Skip non-HTML requests (e.g., API endpoints expecting JSON)
        var acceptHeader = request.Headers.Accept.ToString();
        if (!string.IsNullOrEmpty(acceptHeader) &&
            !acceptHeader.Contains("text/html", StringComparison.OrdinalIgnoreCase) &&
            !acceptHeader.Contains("*/*", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Resolve route — if no match, fall through
        var route = request.Path.Value ?? "/";
        var entry = _ssrRenderer.ResolveRoute(route);
        if (entry == null)
        {
            _logger?.Debug("No route match for {Route} — passing to next middleware", route);
            await _next(context);
            return;
        }

        // Create component context
        var componentContext = new ComponentContext(context);

        // Choose between streaming and standard rendering
        if (_options.Streaming)
        {
            await RenderStreamingAsync(context, route, componentContext);
        }
        else
        {
            await RenderStandardAsync(context, route, componentContext);
        }
    }

    /// <summary>
    /// Renders using the standard (buffered) SSR pipeline.
    /// </summary>
    private async Task RenderStandardAsync(HttpContext context, string route, ComponentContext componentContext)
    {
        try
        {
            var response = await _ssrRenderer.RenderAsync(route, componentContext, context.RequestAborted);
            await response.ExecuteAsync(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no-op
        }
        catch (Exception ex)
        {
            _logger?.Error("Unhandled exception in SSR pipeline for {Route}: {Exception}", route, ex);
            var errorResponse = await _ssrRenderer.RenderErrorAsync(ex);
            await errorResponse.ExecuteAsync(context);
        }
    }

    /// <summary>
    /// Renders using the streaming SSR pipeline.
    /// Flushes chunks progressively to the response body.
    /// </summary>
    private async Task RenderStreamingAsync(HttpContext context, string route, ComponentContext componentContext)
    {
        var response = context.Response;

        try
        {
            // Set response headers for streaming
            response.StatusCode = StatusCodes.Status200OK;
            response.ContentType = "text/html; charset=utf-8";
            // ASP.NET Core handles chunked encoding automatically when Content-Length is not set.
            // Do NOT set Transfer-Encoding: chunked manually — Kestrel manages this.
            response.Headers.CacheControl = "no-cache";

            var cancellationToken = context.RequestAborted;

            await foreach (var chunk in _streamingRenderer.RenderAsyncEnumerable(
                route, componentContext, cancellationToken))
            {
                var bytes = Encoding.UTF8.GetBytes(chunk);
                await response.Body.WriteAsync(bytes, cancellationToken);
                await response.Body.FlushAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected during streaming — no-op
        }
        catch (Exception ex)
        {
            _logger?.Error("Unhandled exception in streaming SSR pipeline for {Route}: {Exception}", route, ex);
            // Attempt to write a minimal error chunk
            try
            {
                var errorHtml = "<script>document.write('An error occurred while rendering this page.');</script>";
                var bytes = Encoding.UTF8.GetBytes(errorHtml);
                await response.Body.WriteAsync(bytes, CancellationToken.None);
                await response.Body.FlushAsync(CancellationToken.None);
            }
            catch
            {
                // Nothing more we can do
            }
        }
    }
}
