using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Isr.Background;
using NextNet.Isr.Cache;
using NextNet.Isr.Configuration;
using NextNet.Isr.Endpoints;
using NextNet.Isr.Manifest;
using NextNet.Isr.Middleware;
using NextNet.Isr.Revalidation;
using NextNet.Logging;
using NextNet.Rendering;

// ReSharper disable once CheckNamespace
namespace NextNet.Isr;

/// <summary>
/// Extension methods for registering NextNet ISR services in the DI container
/// and adding ISR middleware to the application pipeline.
/// </summary>
public static class IsrServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet ISR (Incremental Static Regeneration) services to the
    /// specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure global ISR options.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetIsr(
        this IServiceCollection services,
        Action<IsrGlobalOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Configure global options
        var globalOptions = new IsrGlobalOptions();
        configure?.Invoke(globalOptions);
        globalOptions.Validate();
        services.AddSingleton(globalOptions);

        // Register cache store (default: in-memory)
        services.AddSingleton<IIsrCacheStore, MemoryIsrCacheStore>();

        // Register revalidation components
        services.AddSingleton<IIsrRevalidationManager, IsrRevalidationManager>();
        services.AddSingleton<TimeBasedRevalidator>();
        services.AddSingleton<OnDemandRevalidator>();
        services.AddSingleton<WebhookRevalidator>();

        // Register manifest and generator
        services.AddSingleton<IsrManifestGenerator>();
        services.AddSingleton(sp =>
        {
            var generator = sp.GetRequiredService<IsrManifestGenerator>();
            var manifest = generator.Generate();

            // Validate the manifest
            var errors = IsrConfigValidator.Validate(
                sp.GetRequiredService<IsrGlobalOptions>(), manifest);

            if (errors.Count > 0)
            {
                var logger = sp.GetService<INextNetLogger>();
                foreach (var error in errors)
                {
                    logger?.Warn("ISR configuration error: {Error}", error);
                }
            }

            return manifest;
        });

        // Register the revalidation queue
        services.AddSingleton<RevalidationQueue>(sp =>
        {
            var opts = sp.GetRequiredService<IsrGlobalOptions>();
            return new RevalidationQueue(
                capacity: opts.MaxPendingRevalidations,
                deduplicationWindowSeconds: opts.DeduplicationWindowSeconds,
                maxConcurrentPerRoute: 1);
        });

        // Register background revalidation service
        services.AddHostedService<BackgroundRevalidationService>();

        // Register endpoint handler
        services.AddSingleton<IsrRevalidationEndpoint>();

        // Ensure IHttpContextAccessor is available
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Adds the NextNet ISR middleware and revalidation endpoint middleware
    /// to the application pipeline.
    /// Must be called after <c>UseRouting()</c> and before <c>UseEndpoints()</c>.
    /// The ISR middleware should be placed before the SSR middleware to intercept
    /// cached responses.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is null.</exception>
    public static IApplicationBuilder UseNextNetIsr(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        // Revalidation endpoint must be accessible before ISR middleware intercepts
        app.UseMiddleware<IsrRevalidationMiddleware>();

        // ISR middleware intercepts requests and serves cached/stale content
        app.UseMiddleware<IsrMiddleware>();

        return app;
    }
}
