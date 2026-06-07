using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextNet.Middleware.BuiltIn;

namespace NextNet.Middleware.Extensions;

/// <summary>
/// Extension methods for registering NextNet middleware services and
/// integrating the middleware pipeline with ASP.NET Core.
/// </summary>
public static class MiddlewareServiceCollectionExtensions
{
    /// <summary>
    /// Registers NextNet middleware services and configures the middleware pipeline.
    /// Built-in middleware (Logging, StaticFiles, Compression, ErrorHandling,
    /// Cors, SecurityHeaders) are registered by default with their predefined priorities.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional delegate to customize the middleware pipeline
    /// (e.g., add user middleware, remove built-in middleware defaults).</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetMiddleware(
        this IServiceCollection services,
        Action<MiddlewarePipeline>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Register built-in middleware types as transient so they can be resolved per-request
        services.TryAddTransient<LoggingMiddleware>();
        services.TryAddTransient<StaticFilesMiddleware>();
        services.TryAddTransient<CompressionMiddleware>();
        services.TryAddTransient<ErrorHandlingMiddleware>();
        services.TryAddTransient<CorsMiddleware>();
        services.TryAddTransient<SecurityHeadersMiddleware>();

        // Register options
        services.TryAddSingleton<StaticFilesOptions>();
        services.TryAddSingleton<CompressionOptions>();
        services.TryAddSingleton<ErrorHandlingOptions>();
        services.TryAddSingleton<CorsOptions>();
        services.TryAddSingleton<SecurityHeadersOptions>();

        // Register the pipeline as a singleton so it's built once
        services.AddSingleton<MiddlewarePipeline>(sp =>
        {
            var pipeline = new MiddlewarePipeline();

            // Register built-in middleware with defaults
            pipeline.Use<LoggingMiddleware>();
            pipeline.Use<CorsMiddleware>();
            pipeline.Use<SecurityHeadersMiddleware>();
            pipeline.Use<StaticFilesMiddleware>();
            pipeline.Use<CompressionMiddleware>();
            pipeline.Use<ErrorHandlingMiddleware>();

            // Allow user customization
            configure?.Invoke(pipeline);

            return pipeline;
        });

        return services;
    }

    /// <summary>
    /// Adds the NextNet middleware pipeline to the ASP.NET Core request pipeline.
    /// This should be called after <see cref="AddNextNetMiddleware"/>.
    /// Call this before or after <c>UseNextNet()</c> (SSR) to control ordering.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is null.</exception>
    public static IApplicationBuilder UseNextNetMiddleware(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        var pipeline = app.ApplicationServices.GetRequiredService<MiddlewarePipeline>();

        // Add the NextNet middleware pipeline as ASP.NET Core middleware,
        // chaining its terminal delegate to the next ASP.NET Core middleware.
        app.Use(next =>
        {
            var builtPipeline = pipeline.Build(app.ApplicationServices, next);
            return builtPipeline;
        });

        return app;
    }
}
