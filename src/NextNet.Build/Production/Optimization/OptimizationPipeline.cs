using System.Diagnostics;
using NextNet.Build.Production.Build;
using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.Build.Production.Optimization.CriticalCss;
using NextNet.Build.Production.Optimization.Performance;
using NextNet.IO;

namespace NextNet.Build.Production.Optimization;

/// <summary>
/// Orchestrates all production optimization passes on the build output.
/// </summary>
public partial class OptimizationPipeline
{
    private readonly ISharpFileSystem _fileSystem;
    private readonly BundleAnalyzer _bundleAnalyzer;
    private readonly PerformanceBudgetEvaluator _budgetEvaluator;
    private readonly IEnumerable<IAssetOptimizer> _assetOptimizers;
    private readonly ICriticalCssExtractor? _criticalCssExtractor;

    /// <summary>
    /// Initializes a new instance of <see cref="OptimizationPipeline"/>.
    /// </summary>
    public OptimizationPipeline(
        ISharpFileSystem fileSystem,
        BundleAnalyzer bundleAnalyzer,
        PerformanceBudgetEvaluator budgetEvaluator,
        IEnumerable<IAssetOptimizer> assetOptimizers,
        ICriticalCssExtractor? criticalCssExtractor = null)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
        _bundleAnalyzer = bundleAnalyzer ?? throw new ArgumentNullException(nameof(bundleAnalyzer));
        _budgetEvaluator = budgetEvaluator ?? throw new ArgumentNullException(nameof(budgetEvaluator));
        _assetOptimizers = assetOptimizers ?? throw new ArgumentNullException(nameof(assetOptimizers));
        _criticalCssExtractor = criticalCssExtractor;
    }

    /// <summary>
    /// Runs the full optimization pipeline on the output directory.
    /// </summary>
    /// <param name="outputDirectory">The build output directory.</param>
    /// <param name="options">Production build options.</param>
    /// <returns>An optimization result with metrics and any warnings/errors.</returns>
    public async Task<OptimizationResult> RunAsync(string outputDirectory, ProductionBuildOptions options)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputDirectory));

        var sw = Stopwatch.StartNew();
        var warnings = new List<string>();
        var errors = new List<string>();

        // Phase 1: Analyze current bundle state
        BundleAnalysisResult? analysis = null;
        if (options.AnalyzeBundles)
        {
            try
            {
                analysis = await _bundleAnalyzer.AnalyzeAsync(outputDirectory);
            }
            catch (Exception ex)
            {
                warnings.Add($"Bundle analysis failed: {ex.Message}");
            }
        }

        // Phase 2: Optimize assets (CSS, JS, SVG, images)
        var bytesSaved = 0L;
        var optimizers = _assetOptimizers.ToList();

        if (optimizers.Count > 0)
        {
            var files = CollectFiles(outputDirectory);

            var batches = files
                .GroupBy(f => Path.GetExtension(f))
                .ToDictionary(g => g.Key, g => g.ToList());

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = options.MaxParallelism,
            };

            foreach (var optimizer in optimizers)
            {
                foreach (var batch in batches)
                {
                    var ext = batch.Key;
                    if (!optimizer.CanHandle(ext))
                        continue;

                    await Parallel.ForEachAsync(batch.Value, parallelOptions, async (file, ct) =>
                    {
                        try
                        {
                            var saved = await optimizer.OptimizeAsync(file);
                            Interlocked.Add(ref bytesSaved, saved);
                        }
                        catch (Exception ex)
                        {
                            lock (warnings)
                            {
                                warnings.Add($"Optimization failed for '{file}': {ex.Message}");
                            }
                        }
                    });
                }
            }
        }

        // Phase 3: Critical CSS extraction
        if (options.ExtractCriticalCss && _criticalCssExtractor != null)
        {
            try
            {
                await ExtractCriticalCssAsync(outputDirectory);
            }
            catch (Exception ex)
            {
                warnings.Add($"Critical CSS extraction failed: {ex.Message}");
            }
        }

        // Phase 4: Pre-compression
        if (options.PreCompressAssets)
        {
            try
            {
                var compressed = await PreCompressAsync(outputDirectory);
                bytesSaved += compressed;
            }
            catch (Exception ex)
            {
                warnings.Add($"Pre-compression failed: {ex.Message}");
            }
        }

        // Phase 5: Content-hashed filenames
        if (options.AssetHashing)
        {
            try
            {
                await ApplyContentHashingAsync(outputDirectory);
            }
            catch (Exception ex)
            {
                warnings.Add($"Content hashing failed: {ex.Message}");
            }
        }

        // Phase 6: Bundle analysis and budget enforcement
        PerformanceReport? perfReport = null;
        if (options.AnalyzeBundles && options.Budgets != null)
        {
            try
            {
                // Re-analyze after optimizations
                var finalAnalysis = await _bundleAnalyzer.AnalyzeAsync(outputDirectory);
                perfReport = await _budgetEvaluator.EvaluateAsync(outputDirectory, options.Budgets);

                if (!perfReport.Passed)
                {
                    foreach (var violation in perfReport.Violations)
                    {
                        if (violation.Severity == BudgetViolationAction.Fail)
                        {
                            errors.Add(violation.Message);
                        }
                        else
                        {
                            warnings.Add(violation.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                warnings.Add($"Budget evaluation failed: {ex.Message}");
            }
        }

        sw.Stop();

        var metrics = new BuildMetrics
        {
            TotalBuildTimeMs = sw.ElapsedMilliseconds,
            TotalOutputSize = analysis?.TotalSize ?? 0,
            BundleSize = analysis?.ByExtension.GetValueOrDefault(".js")?.TotalSize ?? 0 +
                         analysis?.ByExtension.GetValueOrDefault(".mjs")?.TotalSize ?? 0,
            StaticAssetsSize = analysis?.TotalSize ?? 0,
            CompressionRatio = 0, // Set by build pipeline if compression is applied
            BytesSaved = bytesSaved,
            AssetCounts = analysis?.ByExtension.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.Count) ?? new Dictionary<string, int>(),
        };

        return new OptimizationResult
        {
            Success = errors.Count == 0,
            Metrics = metrics,
            Warnings = warnings.AsReadOnly(),
            Errors = errors.AsReadOnly(),
        };
    }

    private async Task ExtractCriticalCssAsync(string outputDirectory)
    {
        if (_criticalCssExtractor == null)
            return;

        foreach (var htmlFile in Directory.EnumerateFiles(outputDirectory, "*.html", SearchOption.AllDirectories))
        {
            var html = await _fileSystem.ReadAllTextAsync(htmlFile);
            var result = await _criticalCssExtractor.ExtractAsync(html);

            if (!string.IsNullOrEmpty(result.ModifiedHtml))
            {
                await _fileSystem.WriteAllTextAsync(htmlFile, result.ModifiedHtml);
            }
        }
    }

    private static async Task<long> PreCompressAsync(string outputDirectory)
    {
        var totalBytesSaved = 0L;

        foreach (var file in Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();

            // Skip already compressed files and images
            if (ext is ".gz" or ".br" or ".zip" or ".png" or ".jpg" or ".jpeg" or ".gif" or ".webp")
                continue;

            var content = await File.ReadAllBytesAsync(file);

            // GZip compression
            var gzPath = file + ".gz";
            if (!File.Exists(gzPath))
            {
                using var gzStream = new FileStream(gzPath, FileMode.Create);
                using var gzip = new System.IO.Compression.GZipStream(
                    gzStream, System.IO.Compression.CompressionLevel.Optimal);
                await gzip.WriteAsync(content);
                totalBytesSaved += content.Length - new FileInfo(gzPath).Length;
            }
        }

        return totalBytesSaved > 0 ? totalBytesSaved : 0;
    }

    private static async Task ApplyContentHashingAsync(string outputDirectory)
    {
        var htmlFiles = Directory.EnumerateFiles(outputDirectory, "*.html", SearchOption.AllDirectories);

        foreach (var htmlFile in htmlFiles)
        {
            var html = await File.ReadAllTextAsync(htmlFile);
            var modified = await HashReferencesInHtmlAsync(html, outputDirectory);
            if (modified != html)
            {
                await File.WriteAllTextAsync(htmlFile, modified);
            }
        }
    }

    private static Task<string> HashReferencesInHtmlAsync(string html, string outputDirectory)
    {
        // Find all <script src="...">, <link rel="stylesheet" href="...">, <img src="...">
        var regex = ReferencePattern();
        var modified = regex.Replace(html, match =>
        {
            var attr = match.Groups[1].Value; // src or href
            var url = match.Groups[2].Value;

            // Skip external URLs
            if (url.StartsWith("http://") || url.StartsWith("https://") || url.StartsWith("//"))
                return match.Value;

            var localPath = Path.Combine(outputDirectory, url.TrimStart('/'));
            if (!File.Exists(localPath))
                return match.Value;

            var hash = ComputeHash(localPath);
            var dotIndex = url.LastIndexOf('.');
            if (dotIndex < 0)
                return match.Value;

            var hashedUrl = url[..dotIndex] + "." + hash + url[dotIndex..];
            return $"{attr}=\"{hashedUrl}\"";
        });

        return Task.FromResult(modified);
    }

    private static string ComputeHash(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(stream);
        return Convert.ToHexString(hashBytes)[..8].ToLowerInvariant();
    }

    [System.Text.RegularExpressions.GeneratedRegex(@"(src|href)=""([^""]+)""", System.Text.RegularExpressions.RegexOptions.Compiled)]
    private static partial System.Text.RegularExpressions.Regex ReferencePattern();

    private static List<string> CollectFiles(string directory)
    {
        var files = new List<string>();
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".gz" or ".br")
                continue;
            files.Add(file);
        }
        return files;
    }
}
