using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Options for the <see cref="CorsMiddleware"/>.
/// </summary>
/// <example>
/// <code>
/// // Allow specific origins with credentials
/// services.Configure&lt;CorsOptions&gt;(options =>
/// {
///     options.AllowedOrigins = new[] { "https://example.com" };
///     options.AllowCredentials = true;
/// });
/// </code>
/// </example>
public sealed record CorsOptions
{
    /// <summary>
    /// Gets or sets the allowed origins (e.g. "https://example.com").
    /// Use "*" to allow any origin. Defaults to "*".
    /// </summary>
    public string[] AllowedOrigins { get; set; } = new[] { "*" };

    /// <summary>
    /// Gets or sets the allowed HTTP methods. Defaults to GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS.
    /// </summary>
    public string[] AllowedMethods { get; set; } = new[]
    {
        "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"
    };

    /// <summary>
    /// Gets or sets the allowed request headers. Defaults to "*".
    /// </summary>
    public string[] AllowedHeaders { get; set; } = new[] { "*" };

    /// <summary>
    /// Gets or sets whether credentials (cookies, auth headers) are allowed.
    /// When true, <c>AllowedOrigins</c> must not be "*".
    /// </summary>
    public bool AllowCredentials { get; set; }

    /// <summary>
    /// Gets or sets the exposed response headers.
    /// </summary>
    public string[] ExposedHeaders { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the max age (seconds) for the preflight cache.
    /// Defaults to 600 (10 minutes).
    /// </summary>
    public int PreflightMaxAgeSeconds { get; set; } = 600;
}

/// <summary>
/// Middleware that applies Cross-Origin Resource Sharing (CORS) headers
/// based on configurable policies. Handles both simple and preflight requests.
/// </summary>
/// <example>
/// <code>
/// // In pipeline configuration:
/// pipeline.Use&lt;CorsMiddleware&gt;();
///
/// // The middleware runs early in the pipeline (MiddlewareOrder.Early)
/// // to handle preflight OPTIONS requests before other middleware.
/// </code>
/// </example>
[MiddlewareOrderAttribute(MiddlewareOrder.Early)]
public sealed class CorsMiddleware : IMiddleware
{
    private readonly CorsOptions _options;
    private readonly ILogger<CorsMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorsMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Optional CORS options.</param>
    public CorsMiddleware(
        ILogger<CorsMiddleware> logger,
        IOptions<CorsOptions>? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new CorsOptions();
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var httpContext = context.HttpContext;
        var request = httpContext.Request;

        var origin = request.Headers.Origin.ToString();

        // Only process if the request has an Origin header (CORS request)
        if (string.IsNullOrEmpty(origin))
        {
            await next(httpContext);
            return;
        }

        // Validate origin against allowed list
        if (!IsOriginAllowed(origin))
        {
            _logger.LogWarning("CORS: Origin '{Origin}' is not allowed.", origin);
            httpContext.Response.StatusCode = 403;
            return;
        }

        // Set CORS response headers
        SetCorsHeaders(httpContext, origin);

        // Handle preflight (OPTIONS) requests
        if (HttpMethods.IsOptions(request.Method))
        {
            httpContext.Response.StatusCode = 204;
            return;
        }

        await next(httpContext);
    }

    private bool IsOriginAllowed(string origin)
    {
        if (_options.AllowedOrigins.Length == 0)
            return false;

        // Wildcard allows any origin
        if (_options.AllowedOrigins.Any(o => o == "*"))
            return true;

        return _options.AllowedOrigins.Any(allowed =>
            string.Equals(allowed, origin, StringComparison.OrdinalIgnoreCase));
    }

    private void SetCorsHeaders(HttpContext httpContext, string origin)
    {
        var headers = httpContext.Response.Headers;

        // If wildcard and no credentials, use "*"
        if (_options.AllowedOrigins.Any(o => o == "*") && !_options.AllowCredentials)
        {
            headers.AccessControlAllowOrigin = "*";
        }
        else
        {
            headers.AccessControlAllowOrigin = origin;
        }

        if (_options.AllowCredentials)
        {
            headers.AccessControlAllowCredentials = "true";
        }

        if (_options.ExposedHeaders.Length > 0)
        {
            headers.AccessControlExposeHeaders = string.Join(", ", _options.ExposedHeaders);
        }

        // Preflight-specific headers
        if (HttpMethods.IsOptions(httpContext.Request.Method))
        {
            if (_options.AllowedMethods.Length > 0)
            {
                headers.AccessControlAllowMethods = string.Join(", ", _options.AllowedMethods);
            }

            if (_options.AllowedHeaders.Length > 0)
            {
                headers.AccessControlAllowHeaders = string.Join(", ", _options.AllowedHeaders);
            }

            if (_options.PreflightMaxAgeSeconds > 0)
            {
                headers.AccessControlMaxAge = _options.PreflightMaxAgeSeconds.ToString();
            }
        }
    }
}
