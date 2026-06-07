using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NextNet.Build.Production;
using NextNet.Build.Production.Build;
using NextNet.Build.Production.Caching;
using NextNet.Build.Production.Compression;
using NextNet.Build.Production.Health;
using NextNet.Build.Production.Logging;
using NextNet.Build.Production.Optimization;
using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.Build.Production.Optimization.Performance;
using NextNet.Build.Production.Security;
using Xunit;

namespace NextNet.Build.Tests.Production;

public class ProductionServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetProduction_RegistersAllServices()
    {
        var services = new ServiceCollection();

        // Add logging infrastructure required by ProductionLogger
        services.AddLogging();

        services.AddNextNetProduction(options =>
        {
            options.OutputDirectory = "test-dist";
            options.ExtractCriticalCss = true;
        });

        var provider = services.BuildServiceProvider();

        // Build services
        Assert.NotNull(provider.GetService<ProductionBuildOptions>());
        Assert.NotNull(provider.GetService<ProductionBuildStep>());
        Assert.NotNull(provider.GetService<BuildReportGenerator>());

        // Compression
        Assert.NotNull(provider.GetService<NextNetCompressionOptions>());

        // Caching
        Assert.NotNull(provider.GetService<CacheHeaderOptions>());
        Assert.NotNull(provider.GetService<ContentHashGenerator>());

        // Security
        Assert.NotNull(provider.GetService<SecurityHeadersOptions>());

        // Health
        Assert.NotNull(provider.GetService<NextNetHealthCheck>());
        Assert.NotNull(provider.GetService<HealthCheckEndpoint>());

        // Logging
        Assert.NotNull(provider.GetService<ProductionLogger>());
        Assert.NotNull(provider.GetService<MetricsCollector>());

        // Optimization
        Assert.NotNull(provider.GetService<BundleAnalyzer>());
        Assert.NotNull(provider.GetService<PerformanceBudgetEvaluator>());
        Assert.NotNull(provider.GetService<OptimizationPipeline>());

        // Asset optimizers
        var optimizers = provider.GetServices<IAssetOptimizer>();
        Assert.NotEmpty(optimizers);
        Assert.Contains(optimizers, o => o is CssMinifier);
        Assert.Contains(optimizers, o => o is JavaScriptMinifier);
        Assert.Contains(optimizers, o => o is SvgOptimizer);
    }

    [Fact]
    public void AddNextNetProduction_WithoutConfigure_RegistersDefaults()
    {
        var services = new ServiceCollection();
        services.AddNextNetProduction();

        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<ProductionBuildOptions>();
        Assert.Equal("dist", options.OutputDirectory);
    }

    [Fact]
    public void AddNextNetProduction_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetProduction());
    }
}
