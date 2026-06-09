using Microsoft.AspNetCore.Http;
using NextNet.Components;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.UI.Rendering.Head;

namespace NextNet.UI.Rendering.Middleware;

/// <summary>
/// Extended SSR middleware that injects theme CSS into the page head before
/// the page is rendered. Extends the standard <c>NextNet.Rendering</c> SSR pipeline
/// with UI theming support.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UiSsrMiddleware"/> wraps the standard SSR rendering pipeline and
/// adds theme CSS injection into the HTML <c>&lt;head&gt;</c>. It uses
/// <see cref="ThemeHeadInjector"/> to generate the theme's CSS custom properties
/// and inserts them before the page content is rendered.
/// </para>
/// <para>
/// This middleware is registered via the <c>UseNextNetUi()</c> extension method
/// and should be placed after the standard <c>UseNextNet()</c> or in its place
/// when UI theming is required.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs:
/// app.UseNextNetUi();
/// </code>
/// </example>
public class UiSsrMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SsrRenderer _ssrRenderer;
    private readonly ThemeHeadInjector _themeInjector;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="UiSsrMiddleware"/>.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="ssrRenderer">The SSR renderer for rendering pages.</param>
    /// <param name="themeInjector">Optional theme head injector for CSS injection.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="next"/> or <paramref name="ssrRenderer"/> is null.</exception>
    public UiSsrMiddleware(
        RequestDelegate next,
        SsrRenderer ssrRenderer,
        ThemeHeadInjector? themeInjector = null,
        INextNetLogger? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _ssrRenderer = ssrRenderer ?? throw new ArgumentNullException(nameof(ssrRenderer));
        _themeInjector = themeInjector ?? new ThemeHeadInjector();
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware for the given HTTP context.
    /// Renders the page via SSR and injects theme CSS into the head.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="context"/> is null.</exception>
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

        var route = request.Path.Value ?? "/";
        var componentContext = new ComponentContext(context);

        // Determine theme name from query or default
        var themeName = ResolveThemeName(context);

        // Inject theme CSS into response before rendering
        var themeStyle = _themeInjector.Inject(themeName);

        // Store the theme name in HttpContext.Items for downstream components
        if (themeName != null)
        {
            context.Items["NextNet.ThemeName"] = themeName;
        }

        // Render the page
        try
        {
            var response = await _ssrRenderer.RenderAsync(route, componentContext, context.RequestAborted);

            // Pass through to next middleware if route was not found
            if (response.StatusCode == StatusCodes.Status404NotFound)
            {
                _logger?.Debug("No route match for {Route} — passing to next middleware", route);
                await _next(context);
                return;
            }

            // Inject theme CSS into the response head
            if (themeStyle != null)
            {
                var html = response.Content.ToHtml();
                var headEndIndex = html.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
                if (headEndIndex >= 0)
                {
                    var modifiedHtml = html.Insert(headEndIndex, themeStyle.ToHtml());
                    response = new HtmlResponse(
                        new RawHtmlContent(modifiedHtml),
                        response.StatusCode,
                        response.CacheControl);
                }
            }

            await response.ExecuteAsync(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            // Client disconnected — no-op
        }
        catch (Exception ex)
        {
            _logger?.Error("Unhandled exception in UiSsrMiddleware for {Route}: {Exception}", route, ex);
            await RenderErrorResponseAsync(context, ex);
        }
    }

    /// <summary>
    /// Renders an error response inline when the SSR pipeline fails.
    /// </summary>
    private static async Task RenderErrorResponseAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "text/html; charset=utf-8";

        var title = "500 — Internal Server Error";
        var message = "An unexpected error occurred while rendering the page.";

        var html = $"<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">" +
                   $"<title>{System.Net.WebUtility.HtmlEncode(title)}</title>" +
                   $"<style>body{{font-family:sans-serif;padding:2rem;}}" +
                   $"h1{{color:#c00;}}pre{{background:#f5f5f5;padding:1rem;overflow:auto;}}</style>" +
                   $"</head><body><h1>{System.Net.WebUtility.HtmlEncode(title)}</h1>" +
                   $"<p>{System.Net.WebUtility.HtmlEncode(message)}</p>" +
                   $"<pre>{System.Net.WebUtility.HtmlEncode(exception.ToString())}</pre></body></html>";

        var bytes = System.Text.Encoding.UTF8.GetBytes(html);
        await context.Response.Body.WriteAsync(bytes, CancellationToken.None);
    }

    /// <summary>
    /// Resolves the theme name for the current request.
    /// Checks query parameter, or returns null to use the default theme.
    /// </summary>
    private static string? ResolveThemeName(HttpContext context)
    {
        if (context.Request.Query.TryGetValue("theme", out var themeQuery))
        {
            return themeQuery.ToString();
        }

        return null;
    }
}
