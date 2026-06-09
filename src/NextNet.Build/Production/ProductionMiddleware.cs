using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NextNet.Build.Production.Caching;
using NextNet.Build.Production.Compression;
using NextNet.Build.Production.Health;
using NextNet.Build.Production.Logging;
using NextNet.Build.Production.Security;

namespace NextNet.Build.Production;

/// <summary>
/// Configures the complete production middleware pipeline for a NextNet application.
/// Pipeline order: SecurityHeaders → Compression → CacheHeaders → Health → Timing → Route
/// </summary>
public static class ProductionMiddleware
{
    /// <summary>
    /// Adds all NextNet production middleware to the application pipeline.
    /// Middleware order is designed for security and performance:
    /// <list type="number">
    /// <item>Security headers</item>
    /// <item>Response compression</item>
    /// <item>Cache headers</item>
    /// <item>Request timing / logging</item>
    /// <item>Health check endpoint</item>
    /// </list>
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configureOptions">Optional delegate to configure production options.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseNextNetProduction(
        this IApplicationBuilder app,
        Action<ProductionMiddlewareOptions>? configureOptions = null)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        var options = new ProductionMiddlewareOptions();
        configureOptions?.Invoke(options);

        // Order is important for security and performance:
        // 1. Request timing (captures metrics early)
        if (options.EnableRequestTiming)
        {
            app.UseMiddleware<RequestTimingMiddleware>();
        }

        // 2. Security headers (set on all responses)
        if (options.EnableSecurityHeaders)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        // 3. Response compression
        if (options.EnableCompression)
        {
            app.UseMiddleware<CompressionMiddleware>();
        }

        // 4. Cache headers
        if (options.EnableCaching)
        {
            app.UseMiddleware<CacheHeadersMiddleware>();
        }

        // 5. Health check endpoint
        if (options.EnableHealthEndpoint)
        {
            app.Map("/_health", healthApp =>
            {
                healthApp.Run(async context =>
                {
                    var healthCheck = app.ApplicationServices.GetService(typeof(HealthCheckEndpoint))
                        as HealthCheckEndpoint;
                    if (healthCheck != null)
                    {
                        await healthCheck.HandleAsync(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 503;
                        await context.Response.WriteAsync("Health check not configured.");
                    }
                });
            });
        }

        return app;
    }
}

/// <summary>
/// Options for configuring the production middleware pipeline.
/// </summary>
public sealed class ProductionMiddlewareOptions
{
    /// <summary>
    /// Whether to enable security headers middleware.
    /// </summary>
    public bool EnableSecurityHeaders { get; set; } = true;

    /// <summary>
    /// Whether to enable response compression middleware.
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Whether to enable cache headers middleware.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Whether to enable request timing middleware.
    /// </summary>
    public bool EnableRequestTiming { get; set; } = true;

    /// <summary>
    /// Whether to register the /_health endpoint.
    /// </summary>
    public bool EnableHealthEndpoint { get; set; } = true;
}
