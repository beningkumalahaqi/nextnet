using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Layouts.Errors;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.Routing;

namespace NextNet.Layouts;

/// <summary>
/// Handles error boundaries during layout rendering.
/// Catches exceptions thrown by page or layout rendering and falls back to
/// the configured error page (e.g. <c>app/error.cs</c>), wrapping it in the
/// layout chain.
/// </summary>
/// <example>
/// <code>
/// var boundary = new ErrorBoundaryRenderer(componentResolver);
/// var services = serviceProvider;
/// var manifest = routeManifest;
/// var result = await boundary.RenderAsync(
///     () => page.Render(),
///     layoutTypes,
///     services,
///     manifest,
///     layoutRenderer);
/// // If page.Render() throws, the error boundary resolves the error page
/// // from manifest.ErrorPage and renders it inside the layout chain.
/// </code>
/// </example>
public sealed class ErrorBoundaryRenderer
{
    private readonly IRouteComponentResolver _componentResolver;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ErrorBoundaryRenderer"/>.
    /// </summary>
    /// <param name="componentResolver">The component resolver used to find error page types.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="componentResolver"/> is <c>null</c>.</exception>
    public ErrorBoundaryRenderer(IRouteComponentResolver componentResolver, INextNetLogger? logger = null)
    {
        _componentResolver = componentResolver ?? throw new ArgumentNullException(nameof(componentResolver));
        _logger = logger;
    }

    /// <summary>
    /// Attempts to render content through a layout chain. If rendering fails,
    /// catches the exception, resolves the error page from the manifest,
    /// renders it with the exception context, and wraps it in the layout chain.
    /// </summary>
    /// <param name="renderContent">
    /// A factory that produces the inner content (typically the page content).
    /// This is invoked lazily so errors are caught inside the boundary.
    /// </param>
    /// <param name="layoutTypes">
    /// Ordered list of layout types from innermost to outermost.
    /// Used both for the happy path and the error fallback.
    /// </param>
    /// <param name="serviceProvider">The DI service provider for resolving component instances.</param>
    /// <param name="manifest">
    /// The route manifest from which the error page entry is resolved.
    /// </param>
    /// <param name="layoutRenderer">
    /// The layout renderer used to compose layouts. If <c>null</c>, a bare error page is returned.
    /// </param>
    /// <returns>
    /// A task containing the rendered HTML content. On error, this is the error page
    /// (wrapped in layouts if possible).
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are <c>null</c>.</exception>
    public async Task<IHtmlContent> RenderAsync(
        Func<Task<IHtmlContent>> renderContent,
        IReadOnlyList<Type> layoutTypes,
        IServiceProvider serviceProvider,
        RouteManifest manifest,
        LayoutRenderer? layoutRenderer = null)
    {
        if (renderContent == null) throw new ArgumentNullException(nameof(renderContent));
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        try
        {
            // Render the inner content (typically the page)
            var content = await renderContent();

            // Wrap in layouts if we have a layout renderer
            if (layoutRenderer != null && layoutTypes.Count > 0)
            {
                return await layoutRenderer.RenderAsync(content, layoutTypes, serviceProvider);
            }

            return content;
        }
        catch (Exception ex)
        {
            _logger?.Error("Error boundary caught exception: {ExceptionType}: {Message}",
                ex.GetType().Name, ex.Message);

            return await RenderErrorAsync(ex, layoutTypes, serviceProvider, manifest, layoutRenderer);
        }
    }

    /// <summary>
    /// Renders the error page for the given exception.
    /// </summary>
    private async Task<IHtmlContent> RenderErrorAsync(
        Exception exception,
        IReadOnlyList<Type> layoutTypes,
        IServiceProvider serviceProvider,
        RouteManifest manifest,
        LayoutRenderer? layoutRenderer)
    {
        // Try to render a user-defined error page
        if (manifest.ErrorPage != null)
        {
            var errorType = _componentResolver.GetPageType(manifest.ErrorPage);
            if (errorType != null && typeof(IErrorPage).IsAssignableFrom(errorType))
            {
                try
                {
                    var errorPage = (IErrorPage)serviceProvider.GetRequiredService(errorType);
                    var errorContent = await errorPage.Render(exception);

                    // Try to wrap the error page in the layout chain.
                    // If layout rendering also fails (e.g. the layout itself is broken),
                    // return the bare error page without layout wrapping.
                    if (layoutRenderer != null && layoutTypes.Count > 0)
                    {
                        try
                        {
                            return await layoutRenderer.RenderAsync(errorContent, layoutTypes, serviceProvider);
                        }
                        catch
                        {
                            // Layout wrapping failed — render the error page without layouts
                            return errorContent;
                        }
                    }

                    return errorContent;
                }
                catch (Exception innerEx)
                {
                    _logger?.Error("[{ErrorCode}] Error page render itself failed: {ExceptionType}: {Message}",
                        LayoutErrorCodes.ErrorPageRenderFailed,
                        innerEx.GetType().Name, innerEx.Message);
                }
            }
        }

        // Built-in fallback
        _logger?.Error("[{ErrorCode}] Error boundary fell back to built-in error page for: {ExceptionType}: {Message}",
            LayoutErrorCodes.ErrorBoundaryFallback,
            exception.GetType().Name, exception.Message);
        return GenerateBuiltInErrorContent(exception);
    }

    /// <summary>
    /// Generates a hardcoded minimal error page as the last resort.
    /// </summary>
    private static IHtmlContent GenerateBuiltInErrorContent(Exception exception)
    {
        var title = "500 — Internal Server Error";
        var message = exception is RenderException re
            ? re.Message
            : "An unexpected error occurred while rendering the page.";

        var stackTrace = exception.ToString();
        var encodedTitle = System.Net.WebUtility.HtmlEncode(title);
        var encodedMessage = System.Net.WebUtility.HtmlEncode(message);
        var encodedStackTrace = System.Net.WebUtility.HtmlEncode(stackTrace);

        var html = $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
                   $"<title>{encodedTitle}</title>" +
                   $"<style>body{{font-family:sans-serif;padding:2rem;}}" +
                   $"h1{{color:#c00;}}pre{{background:#f5f5f5;padding:1rem;overflow:auto;}}</style>" +
                   $"</head><body><h1>{encodedTitle}</h1><p>{encodedMessage}</p>" +
                   $"<pre>{encodedStackTrace}</pre></body></html>";

        return new RawHtmlContent(html);
    }
}
