using NextNet.Build.Production.Build;
using NextNet.Build.Production.Optimization;
using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.Build.Production.Optimization.Performance;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.Production.Integration;

public class ProductionPipelineIntegrationTests
{
    [Fact]
    public async Task FullPipeline_Should_RunSuccessfully_When_SampleOutputProvided()
    {
        var fs = new DefaultSharpFileSystem();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "index.html"),
                "<!DOCTYPE html>\n<html>\n<head>\n    <title>Test</title>\n    " +
                "<link rel=\"stylesheet\" href=\"styles.css\">\n</head>\n" +
                "<body>\n    <h1>Hello</h1>\n</body>\n</html>");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "styles.css"),
                "/* Main styles */\nbody { margin: 0; padding: 0; }\nh1 { color: blue; }");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "app.js"),
                "// JavaScript\nvar x = 1;\nvar y = 2;\nconsole.log(x + y);");
            await File.WriteAllTextAsync(Path.Combine(tempDir, "image.svg"),
                "<?xml version=\"1.0\"?><svg width=\"100\" height=\"100\"><circle cx=\"50\" cy=\"50\" r=\"40\"/></svg>");

            var analyzer = new BundleAnalyzer(fs);
            var evaluator = new PerformanceBudgetEvaluator(fs);
            var optimizers = new IAssetOptimizer[]
            {
                new CssMinifier(fs),
                new JavaScriptMinifier(fs),
                new SvgOptimizer(fs),
                new ImageOptimizer(fs),
            };

            var pipeline = new OptimizationPipeline(fs, analyzer, evaluator, optimizers);
            var buildStep = new ProductionBuildStep(fs, pipeline, new BuildReportGenerator(fs));

            var options = new ProductionBuildOptions
            {
                OutputDirectory = tempDir,
                MinifyCss = true,
                MinifyJavaScript = true,
                OptimizeSvg = true,
                PreCompressAssets = true,
                AssetHashing = true,
                GenerateBuildReport = true,
                AnalyzeBundles = true,
            };

            var result = await buildStep.ExecuteAsync(options);

            Assert.True(result.Success, string.Join(", ", result.Errors));
            Assert.NotNull(result.Metrics);
            Assert.True(result.Metrics.TotalBuildTimeMs >= 0);
            Assert.True(result.Metrics.TotalOutputSize > 0);

            Assert.True(File.Exists(Path.Combine(tempDir, "index.html")));
            Assert.True(File.Exists(Path.Combine(tempDir, "styles.css")));
            Assert.True(File.Exists(Path.Combine(tempDir, "app.js")));
            Assert.True(File.Exists(Path.Combine(tempDir, "_optimizationReport.json")));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task FullPipeline_Should_EnforceBudgets_When_PerformanceBudgetsConfigured()
    {
        var fs = new DefaultSharpFileSystem();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            await File.WriteAllTextAsync(Path.Combine(tempDir, "large.js"), new string('x', 50000));

            var analyzer = new BundleAnalyzer(fs);
            var evaluator = new PerformanceBudgetEvaluator(fs);
            var pipeline = new OptimizationPipeline(fs, analyzer, evaluator, Array.Empty<IAssetOptimizer>());

            var options = new ProductionBuildOptions
            {
                OutputDirectory = tempDir,
                AnalyzeBundles = true,
                Budgets = new PerformanceBudgets
                {
                    JavaScriptSize = 1000,
                    Action = BudgetViolationAction.Warn,
                },
            };

            var result = await pipeline.RunAsync(tempDir, options);

            Assert.True(result.Success);
            Assert.NotEmpty(result.Warnings);
            Assert.Contains(result.Warnings, w => w.Contains("JavaScript size"));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task FullPipeline_Should_NotCrash_When_OutputDirectoryMissing()
    {
        var fs = new DefaultSharpFileSystem();
        var analyzer = new BundleAnalyzer(fs);
        var evaluator = new PerformanceBudgetEvaluator(fs);
        var pipeline = new OptimizationPipeline(fs, analyzer, evaluator, Array.Empty<IAssetOptimizer>());

        var options = new ProductionBuildOptions
        {
            OutputDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()),
        };

        var result = await pipeline.RunAsync(options.OutputDirectory, options);

        Assert.NotNull(result);
    }
}
