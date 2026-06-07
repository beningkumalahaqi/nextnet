using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware.BuiltIn;

/// <summary>
/// Options for the <see cref="SecurityHeadersMiddleware"/>.
/// </summary>
public class SecurityHeadersOptions
{
    /// <summary>
    /// Gets or sets the Content-Security-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? ContentSecurityPolicy { get; set; } =
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data:; font-src 'self'; connect-src 'self'";

    /// <summary>
    /// Gets or sets the Strict-Transport-Security header value.
    /// When null, the header is not set.
    /// </summary>
    public string? StrictTransportSecurity { get; set; } =
        "max-age=31536000; includeSubDomains";

    /// <summary>
    /// Gets or sets the X-Frame-Options header value.
    /// When null, the header is not set.
    /// </summary>
    public string? XFrameOptions { get; set; } = "DENY";

    /// <summary>
    /// Gets or sets the X-Content-Type-Options header value.
    /// When null, the header is not set.
    /// </summary>
    public string? XContentTypeOptions { get; set; } = "nosniff";

    /// <summary>
    /// Gets or sets the Referrer-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    /// <summary>
    /// Gets or sets the Permissions-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? PermissionsPolicy { get; set; } =
        "camera=(), microphone=(), geolocation=(), interest-cohort=()";

    /// <summary>
    /// Gets or sets the X-Permitted-Cross-Domain-Policies header value.
    /// When null, the header is not set.
    /// </summary>
    public string? XPermittedCrossDomainPolicies { get; set; } = "none";

    /// <summary>
    /// Gets or sets the Cross-Origin-Embedder-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? CrossOriginEmbedderPolicy { get; set; }

    /// <summary>
    /// Gets or sets the Cross-Origin-Opener-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? CrossOriginOpenerPolicy { get; set; }

    /// <summary>
    /// Gets or sets the Cross-Origin-Resource-Policy header value.
    /// When null, the header is not set.
    /// </summary>
    public string? CrossOriginResourcePolicy { get; set; }
}

/// <summary>
/// Middleware that sets security-related HTTP headers on all responses.
/// Configurable via <see cref="SecurityHeadersOptions"/>.
/// </summary>
[MiddlewareOrderAttribute(MiddlewareOrder.Early + 10)]
public class SecurityHeadersMiddleware : IMiddleware
{
    private readonly SecurityHeadersOptions _options;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersMiddleware"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="options">Optional security headers options.</param>
    public SecurityHeadersMiddleware(
        ILogger<SecurityHeadersMiddleware> logger,
        IOptions<SecurityHeadersOptions>? options = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? new SecurityHeadersOptions();
    }

    /// <inheritdoc />
    public async Task InvokeAsync(MiddlewareContext context, RequestDelegate next)
    {
        var httpContext = context.HttpContext;
        var headers = httpContext.Response.Headers;

        // Set security headers before the response starts
        if (_options.ContentSecurityPolicy != null)
            headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;

        if (_options.StrictTransportSecurity != null)
            headers["Strict-Transport-Security"] = _options.StrictTransportSecurity;

        if (_options.XFrameOptions != null)
            headers["X-Frame-Options"] = _options.XFrameOptions;

        if (_options.XContentTypeOptions != null)
            headers["X-Content-Type-Options"] = _options.XContentTypeOptions;

        if (_options.ReferrerPolicy != null)
            headers["Referrer-Policy"] = _options.ReferrerPolicy;

        if (_options.PermissionsPolicy != null)
            headers["Permissions-Policy"] = _options.PermissionsPolicy;

        if (_options.XPermittedCrossDomainPolicies != null)
            headers["X-Permitted-Cross-Domain-Policies"] = _options.XPermittedCrossDomainPolicies;

        if (_options.CrossOriginEmbedderPolicy != null)
            headers["Cross-Origin-Embedder-Policy"] = _options.CrossOriginEmbedderPolicy;

        if (_options.CrossOriginOpenerPolicy != null)
            headers["Cross-Origin-Opener-Policy"] = _options.CrossOriginOpenerPolicy;

        if (_options.CrossOriginResourcePolicy != null)
            headers["Cross-Origin-Resource-Policy"] = _options.CrossOriginResourcePolicy;

        await next(httpContext);
    }
}
