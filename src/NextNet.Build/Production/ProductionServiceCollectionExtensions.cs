using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.Production.Build;
using NextNet.Build.Production.Caching;
using NextNet.Build.Production.Compression;
using NextNet.Build.Production.Health;
using NextNet.Build.Production.Logging;
using NextNet.Build.Production.Optimization;
using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.Build.Production.Optimization.CriticalCss;
using NextNet.Build.Production.Optimization.Performance;
using NextNet.Build.Production.Security;
using NextNet.IO;

namespace NextNet.Build.Production;

/// <summary>
/// Extension methods for registering NextNet production services in the DI container.
/// </summary>
public static class ProductionServiceCollectionExtensions
{
    /// <summary>
    /// Adds all NextNet production services to the service collection.
    /// Includes compression, caching, security, health checks, logging,
    /// optimization pipeline, and asset optimizers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Optional delegate to configure production build options.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddNextNetProduction(
        this IServiceCollection services,
        Action<ProductionBuildOptions>? configureOptions = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register file system abstraction
        services.AddSingleton<ISharpFileSystem>(_ => new DefaultSharpFileSystem());

        // Configure production build options
        var buildOptions = new ProductionBuildOptions();
        configureOptions?.Invoke(buildOptions);
        services.AddSingleton(buildOptions);

        // -----------------------------------------------------------------
        // Compression
        // -----------------------------------------------------------------
        services.AddSingleton<NextNetCompressionOptions>(_ => new NextNetCompressionOptions
        {
            EnableCompression = buildOptions.PreCompressAssets,
            PreCompressAssets = buildOptions.PreCompressAssets,
        });

        // -----------------------------------------------------------------
        // Caching
        // -----------------------------------------------------------------
        services.AddSingleton<CacheHeaderOptions>(_ => new CacheHeaderOptions
        {
            EnableCaching = true,
        });
        services.AddSingleton<ContentHashGenerator>();

        // -----------------------------------------------------------------
        // Security
        // -----------------------------------------------------------------
        services.AddSingleton<SecurityHeadersOptions>(_ =>
        {
            var options = new SecurityHeadersOptions();
            if (buildOptions.ExtractCriticalCss)
            {
                // If critical CSS is enabled, we need 'unsafe-inline' for styles
                // until the deferred CSS loads
                options.ContentSecurityPolicy = ContentSecurityPolicyBuilder.CreateDefault()
                    .WithStyleSrc("'self'", "'unsafe-inline'")
                    .Build();
            }
            return options;
        });

        // -----------------------------------------------------------------
        // Health
        // -----------------------------------------------------------------
        services.AddSingleton<NextNetHealthCheck>();
        services.AddTransient<HealthCheckEndpoint>();

        // -----------------------------------------------------------------
        // Logging
        // -----------------------------------------------------------------
        services.AddSingleton<ProductionLogger>();
        services.AddSingleton<MetricsCollector>();

        // -----------------------------------------------------------------
        // Optimization Pipeline
        // -----------------------------------------------------------------
        services.AddSingleton<BundleAnalyzer>();
        services.AddSingleton<PerformanceBudgetEvaluator>();
        services.AddSingleton<BuildReportGenerator>();

        // Register asset optimizers
        services.AddSingleton<IAssetOptimizer, CssMinifier>();
        services.AddSingleton<IAssetOptimizer, JavaScriptMinifier>();
        services.AddSingleton<IAssetOptimizer, SvgOptimizer>();
        services.AddSingleton<IAssetOptimizer, ImageOptimizer>();

        // Register critical CSS extractor (if needed)
        if (buildOptions.ExtractCriticalCss)
        {
            services.AddSingleton<ICriticalCssExtractor, CriticalCssExtractor>();
        }

        // Register the optimization pipeline
        services.AddSingleton<OptimizationPipeline>();

        // Register the production build step
        services.AddTransient<ProductionBuildStep>();

        return services;
    }
}
