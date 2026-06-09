using Microsoft.AspNetCore.Http;

namespace NextNet.Build.Production.Security;

/// <summary>
/// Middleware that adds security headers to all HTTP responses.
/// Implements OWASP recommended headers by default.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SecurityHeadersOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="SecurityHeadersMiddleware"/>.
    /// </summary>
    public SecurityHeadersMiddleware(
        RequestDelegate next,
        SecurityHeadersOptions options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Processes the request and adds security headers to the response.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.EnableSecurityHeaders)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (!_options.ExcludedPaths.Contains(path))
        {
            AddSecurityHeaders(context);
        }

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        // X-Frame-Options: Prevent clickjacking
        if (!string.IsNullOrEmpty(_options.XFrameOptions))
        {
            context.Response.Headers["X-Frame-Options"] = _options.XFrameOptions;
        }

        // X-Content-Type-Options: Prevent MIME sniffing
        if (!string.IsNullOrEmpty(_options.XContentTypeOptions))
        {
            context.Response.Headers["X-Content-Type-Options"] = _options.XContentTypeOptions;
        }

        // X-XSS-Protection: Enable browser XSS filter
        if (!string.IsNullOrEmpty(_options.XssProtection))
        {
            context.Response.Headers["X-XSS-Protection"] = _options.XssProtection;
        }

        // Referrer-Policy: Control referrer information
        if (!string.IsNullOrEmpty(_options.ReferrerPolicy))
        {
            context.Response.Headers["Referrer-Policy"] = _options.ReferrerPolicy;
        }

        // Permissions-Policy: Limit browser features
        if (!string.IsNullOrEmpty(_options.PermissionsPolicy))
        {
            context.Response.Headers["Permissions-Policy"] = _options.PermissionsPolicy;
        }

        // Content-Security-Policy: Prevent XSS and data injection
        if (!string.IsNullOrEmpty(_options.ContentSecurityPolicy))
        {
            context.Response.Headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
        }

        // HSTS: Force HTTPS
        if (_options.EnableHsts && context.Request.IsHttps)
        {
            var hstsValue = $"max-age={_options.HstsMaxAgeDays * 86400}";
            if (_options.HstsIncludeSubDomains)
                hstsValue += "; includeSubDomains";
            if (_options.HstsPreload)
                hstsValue += "; preload";

            context.Response.Headers["Strict-Transport-Security"] = hstsValue;
        }

        // Custom headers
        foreach (var kvp in _options.CustomHeaders)
        {
            context.Response.Headers[kvp.Key] = kvp.Value;
        }
    }
}
