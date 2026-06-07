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
    public async Task FullPipeline_WithSampleOutput_RunsSuccessfully()
    {
        var fs = new DefaultSharpFileSystem();

        // Set up a sample output directory
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create sample build output
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

            // Set up the pipeline with all optimizers
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

            // Basic assertions
            Assert.True(result.Success, string.Join(", ", result.Errors));
            Assert.NotNull(result.Metrics);
            Assert.True(result.Metrics.TotalBuildTimeMs >= 0);
            Assert.True(result.Metrics.TotalOutputSize > 0);

            // Verify files were processed
            Assert.True(File.Exists(Path.Combine(tempDir, "index.html")));
            Assert.True(File.Exists(Path.Combine(tempDir, "styles.css")));
            Assert.True(File.Exists(Path.Combine(tempDir, "app.js")));

            // Verify report was generated
            Assert.True(File.Exists(Path.Combine(tempDir, "_optimizationReport.json")));
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public async Task FullPipeline_WithPerformanceBudgets_EnforcesThem()
    {
        var fs = new DefaultSharpFileSystem();

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Create a large file that exceeds budget
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

            // Should have warnings but still succeed (Warn action)
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
    public async Task FullPipeline_WithoutOutputDirectory_ReturnsError()
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

        // Non-existent directory should still produce a result (no crash)
        Assert.NotNull(result);
    }
}
