using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Logging;
using NextNet.Rendering.Errors;
using NextNet.Routing;
using NextNet.Routing.Models;

namespace NextNet.Rendering;

/// <summary>
/// Main server-side rendering pipeline.
/// Resolves routes, instantiates page components, composes layout chains,
/// and produces <see cref="HtmlResponse"/> results.
/// </summary>
/// <example>
/// <code>
/// // Register in DI and use in Minimal API:
/// var app = builder.Build();
/// app.MapGet("/{**path}", async (SsrRenderer renderer, HttpContext ctx) =>
/// {
///     var context = new ComponentContext(ctx);
///     return await renderer.RenderAsync(ctx.Request.Path.Value ?? "/", context);
/// });
/// </code>
/// </example>
public sealed class SsrRenderer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RouteManifest _routeManifest;
    private readonly IRouteComponentResolver _componentResolver;
    private readonly LayoutRenderer _layoutRenderer;
    private readonly INextNetLogger? _logger;
    private readonly SsrOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="SsrRenderer"/>.
    /// </summary>
    /// <param name="serviceProvider">The DI service provider for resolving components.</param>
    /// <param name="routeManifest">The route manifest containing all discovered routes.</param>
    /// <param name="options">SSR configuration options. If null, defaults are used.</param>
    /// <param name="componentResolver">Custom component type resolver. If null, convention-based resolution is used.</param>
    /// <param name="layoutRenderer">Custom layout renderer. If null, a default instance is created.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> or <paramref name="routeManifest"/> is null.</exception>
    public SsrRenderer(
        IServiceProvider serviceProvider,
        RouteManifest routeManifest,
        SsrOptions? options = null,
        IRouteComponentResolver? componentResolver = null,
        LayoutRenderer? layoutRenderer = null,
        INextNetLogger? logger = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _routeManifest = routeManifest ?? throw new ArgumentNullException(nameof(routeManifest));
        _options = options ?? new SsrOptions();
        _componentResolver = componentResolver ?? new ConventionRouteComponentResolver(logger: logger);
        _layoutRenderer = layoutRenderer ?? new LayoutRenderer(_componentResolver, logger);
        _logger = logger;
    }

    /// <summary>
    /// Renders the page matching the given route and returns an <see cref="HtmlResponse"/>.
    /// </summary>
    /// <param name="route">The route path (e.g. <c>"/about"</c> or <c>"/blog/post-1"</c>).</param>
    /// <param name="context">The component context for the current request.</param>
    /// <param name="cancellationToken">Optional cancellation token for cooperating with request aborts or timeouts.</param>
    /// <returns>An <see cref="HtmlResponse"/> containing the rendered HTML.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/> or <paramref name="context"/> is null.</exception>
    /// <exception cref="RenderException">Thrown when a render error occurs during execution.</exception>
    public async Task<HtmlResponse> RenderAsync(
        string route,
        ComponentContext context,
        CancellationToken cancellationToken = default)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (context == null) throw new ArgumentNullException(nameof(context));

        // Combine timeout token with the provided cancellation token
        using var timeoutCts = new CancellationTokenSource(_options.RenderTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, cancellationToken);
        var linkedToken = linkedCts.Token;

        try
        {
            // 1. Resolve route
            var entry = ResolveRoute(route);
            if (entry == null)
            {
                _logger?.Debug("[{ErrorCode}] No route found for {Route}", Errors.RenderingErrorCodes.RouteNotFound, route);
                return HtmlResponse.NotFound();
            }

            _logger?.Info("Rendering route {Route} -> {FilePath}", route, entry.FilePath);

            linkedToken.ThrowIfCancellationRequested();

            // 2. Instantiate and render the page component with cancellation
            var pageContent = await RenderPageAsync(entry, context, linkedToken);

            // 3. Compose the layout chain (innermost -> outermost)
            var finalContent = await _layoutRenderer.RenderAsync(
                pageContent, entry.LayoutChain, _serviceProvider);

            // 4. Determine caching strategy
            var cacheControl = GetCacheControl(entry);

            // 5. Return the HTML response
            return new HtmlResponse(finalContent, StatusCodes.Status200OK, cacheControl);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _logger?.Warn("Request was cancelled for route {Route}", route);
            throw; // Re-throw request cancellations
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            _logger?.Error("[{ErrorCode}] Render timed out for route {Route}", Errors.RenderingErrorCodes.RenderTimeout, route);
            return await RenderErrorAsync(
                new RenderException($"[{Errors.RenderingErrorCodes.RenderTimeout}] Render timed out after {_options.RenderTimeout.TotalSeconds}s"));
        }
        catch (Exception ex)
        {
            _logger?.Error("Failed to render route {Route}: {Exception}", route, ex);
            return await RenderErrorAsync(ex);
        }
    }

    /// <summary>
    /// Renders the page matching the given route and returns the raw <see cref="IHtmlContent"/>.
    /// Useful for testing or embedding in other responses.
    /// </summary>
    public async Task<IHtmlContent> RenderContentAsync(
        string route,
        ComponentContext context,
        CancellationToken cancellationToken = default)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var entry = ResolveRoute(route)
            ?? throw new RenderException($"[{Errors.RenderingErrorCodes.RouteNotFound}] No route found for '{route}'.");

        var pageContent = await RenderPageAsync(entry, context, cancellationToken);
        return await _layoutRenderer.RenderAsync(
            pageContent, entry.LayoutChain, _serviceProvider);
    }

    /// <summary>
    /// Gets the <see cref="SsrOptions"/> currently in use.
    /// </summary>
    public SsrOptions Options => _options;

    /// <summary>
    /// Gets the component resolver used by this renderer.
    /// </summary>
    internal IRouteComponentResolver ComponentResolver => _componentResolver;

    /// <summary>
    /// Gets the layout renderer used by this renderer.
    /// </summary>
    internal LayoutRenderer LayoutRendererInstance => _layoutRenderer;

    /// <summary>
    /// Gets the service provider used by this renderer for resolving components.
    /// </summary>
    internal IServiceProvider ServiceProvider => _serviceProvider;

    /// <summary>
    /// Resolves a route string to a <see cref="RouteEntry"/> from the manifest.
    /// Supports exact matches and parameterised route matching.
    /// </summary>
    internal RouteEntry? ResolveRoute(string route)
    {
        if (string.IsNullOrEmpty(route))
            return null;

        // Normalize: ensure leading slash
        var normalized = route.StartsWith('/') ? route : "/" + route;

        // 1. Exact match
        var exact = _routeManifest.Pages.FirstOrDefault(
            p => string.Equals(p.RoutePattern, normalized, StringComparison.OrdinalIgnoreCase));
        if (exact != null)
            return exact;

        // 2. Parameterised match (simple prefix-based for now)
        // e.g. route "/blog/hello" matches pattern "/blog/{slug}"
        foreach (var page in _routeManifest.Pages)
        {
            if (page.SegmentKind == RouteSegmentKind.Dynamic && MatchesPattern(page.RoutePattern, normalized))
            {
                return page;
            }
        }

        return null;
    }

    /// <summary>
    /// Instantiates the page component for the given entry and renders it.
    /// </summary>
    internal async Task<IHtmlContent> RenderPageAsync(
        RouteEntry entry,
        ComponentContext context,
        CancellationToken cancellationToken = default)
    {
        var pageType = _componentResolver.GetPageType(entry)
            ?? throw new RenderException($"[{Errors.RenderingErrorCodes.PageTypeNotResolved}] Cannot resolve page type for route: {entry.RoutePattern} (file: {entry.FilePath})");

        IPage page;
        try
        {
            page = (IPage)_serviceProvider.GetRequiredService(pageType);
        }
        catch (InvalidOperationException ex)
        {
            throw new RenderException(
                $"[{Errors.RenderingErrorCodes.PageTypeNotRegistered}] Page type '{pageType.FullName}' is not registered in the DI container. " +
                "Ensure the component is registered via services.AddScoped<IPage, ...>().", ex);
        }

        cancellationToken.ThrowIfCancellationRequested();

        // If the page implements IComponentContextAware, provide the component context
        // before rendering. This enables access to Route and Context on the Page base class.
        if (page is IComponentContextAware contextAware)
        {
            contextAware.SetContext(context);
        }

        _logger?.Debug("Rendering page {PageType}", pageType.Name);

        // Use Task.WhenAny to cooperatively observe cancellation
        // since IPage.Render() may not accept a CancellationToken
        var renderTask = page.Render();
        var completedTask = await Task.WhenAny(renderTask, Task.Delay(Timeout.Infinite, cancellationToken));

        if (completedTask != renderTask)
        {
            // Cancellation was requested; the render task is abandoned
            throw new OperationCanceledException(cancellationToken);
        }

        return await renderTask;
    }

    /// <summary>
    /// Generates an error <see cref="HtmlResponse"/> from the given exception.
    /// Attempts to resolve an error page from the manifest; falls back to built-in.
    /// </summary>
    internal async Task<HtmlResponse> RenderErrorAsync(Exception exception)
    {
        // Try to use a user-defined error page
        if (_routeManifest.ErrorPage != null)
        {
            var errorType = _componentResolver.GetPageType(_routeManifest.ErrorPage);
            if (errorType != null && typeof(IErrorPage).IsAssignableFrom(errorType))
            {
                try
                {
                    var errorPage = (IErrorPage)_serviceProvider.GetRequiredService(errorType);
                    var content = await errorPage.Render(exception);
                    return new HtmlResponse(content, StatusCodes.Status500InternalServerError, "no-store");
                }
                catch (Exception innerEx)
                {
                    _logger?.Error("Error page render failed: {Exception}", innerEx);
                }
            }
        }

        // Built-in fallback error page
        var errorHtml = GenerateBuiltInErrorPage(exception);
        return new HtmlResponse(errorHtml, StatusCodes.Status500InternalServerError, "no-store");
    }

    /// <summary>
    /// Determines the Cache-Control header value based on route type.
    /// </summary>
    internal static string GetCacheControl(RouteEntry entry)
    {
        return entry.Type switch
        {
            RouteType.Page => "public, max-age=3600",
            RouteType.Api => "no-cache",
            RouteType.Error => "no-store",
            _ => "no-cache",
        };
    }

    private static bool MatchesPattern(string pattern, string route)
    {
        // Simple pattern matching: split by '/', compare segments
        var patternSegments = pattern.Trim('/').Split('/');
        var routeSegments = route.Trim('/').Split('/');

        if (patternSegments.Length != routeSegments.Length)
            return false;

        for (int i = 0; i < patternSegments.Length; i++)
        {
            var ps = patternSegments[i];
            if (ps.StartsWith('{') && ps.EndsWith('}'))
                continue; // Dynamic segment matches anything
            if (!string.Equals(ps, routeSegments[i], StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static IHtmlContent GenerateBuiltInErrorPage(Exception exception)
    {
        var title = "500 — Internal Server Error";
        var message = exception is RenderException re
            ? re.Message
            : "An unexpected error occurred while rendering the page.";

        var stackTrace = exception.ToString();

        return new ErrorHtmlContent(title, message, stackTrace);
    }

    /// <summary>
    /// Internal IHtmlContent for the built-in error page response.
    /// </summary>
    private sealed class ErrorHtmlContent : Components.IHtmlContent
    {
        private readonly string _title;
        private readonly string _message;
        private readonly string _stackTrace;

        public ErrorHtmlContent(string title, string message, string stackTrace)
        {
            _title = title;
            _message = message;
            _stackTrace = stackTrace;
        }

        public Task WriteToAsync(TextWriter writer)
        {
            return writer.WriteAsync(ToHtml());
        }

        public string ToHtml()
        {
            var encodedTitle = System.Net.WebUtility.HtmlEncode(_title);
            var encodedMessage = System.Net.WebUtility.HtmlEncode(_message);
            var encodedStackTrace = System.Net.WebUtility.HtmlEncode(_stackTrace);

            return $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
                   $"<title>{encodedTitle}</title>" +
                   $"<style>body{{font-family:sans-serif;padding:2rem;}}" +
                   $"h1{{color:#c00;}}pre{{background:#f5f5f5;padding:1rem;overflow:auto;}}</style>" +
                   $"</head><body><h1>{encodedTitle}</h1><p>{encodedMessage}</p>" +
                   $"<pre>{encodedStackTrace}</pre></body></html>";
        }
    }
}
