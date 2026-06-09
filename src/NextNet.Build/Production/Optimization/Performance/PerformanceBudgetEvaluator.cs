using NextNet.IO;

namespace NextNet.Build.Production.Optimization.Performance;

/// <summary>
/// Evaluates performance budgets against the build output directory.
/// </summary>
public sealed class PerformanceBudgetEvaluator
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="PerformanceBudgetEvaluator"/>.
    /// </summary>
    public PerformanceBudgetEvaluator(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Evaluates the given budgets against the build output directory.
    /// </summary>
    /// <param name="outputDirectory">The build output directory to scan.</param>
    /// <param name="budgets">The performance budgets to evaluate.</param>
    /// <returns>A performance report.</returns>
    public async Task<PerformanceReport> EvaluateAsync(string outputDirectory, PerformanceBudgets budgets)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputDirectory));
        if (budgets == null)
            throw new ArgumentNullException(nameof(budgets));

        var report = new PerformanceReport();
        var violations = new List<BudgetViolation>();

        // Gather file sizes by category
        var totalSize = 0L;
        var jsSize = 0L;
        var cssSize = 0L;
        var imageSize = 0L;
        var requestCount = 0;

        await foreach (var file in EnumerateFilesAsync(outputDirectory))
        {
            var fileInfo = new FileInfo(file);
            totalSize += fileInfo.Length;
            requestCount++;

            var ext = Path.GetExtension(file).ToLowerInvariant();
            switch (ext)
            {
                case ".js":
                case ".mjs":
                    jsSize += fileInfo.Length;
                    break;
                case ".css":
                    cssSize += fileInfo.Length;
                    break;
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".gif":
                case ".webp":
                case ".svg":
                case ".ico":
                    imageSize += fileInfo.Length;
                    break;
            }
        }

        report.TotalSize = totalSize;
        report.JavaScriptSize = jsSize;
        report.CssSize = cssSize;
        report.ImageSize = imageSize;
        report.RequestCount = requestCount;

        // Evaluate budgets
        if (budgets.TotalSize.HasValue && totalSize > budgets.TotalSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "TotalSize",
                Expected: FormatSize(budgets.TotalSize.Value),
                Actual: FormatSize(totalSize),
                Severity: budgets.Action,
                Message: $"Total output size {FormatSize(totalSize)} exceeds budget of {FormatSize(budgets.TotalSize.Value)}."
            ));
        }

        if (budgets.JavaScriptSize.HasValue && jsSize > budgets.JavaScriptSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "JavaScriptSize",
                Expected: FormatSize(budgets.JavaScriptSize.Value),
                Actual: FormatSize(jsSize),
                Severity: budgets.Action,
                Message: $"JavaScript size {FormatSize(jsSize)} exceeds budget of {FormatSize(budgets.JavaScriptSize.Value)}."
            ));
        }

        if (budgets.CssSize.HasValue && cssSize > budgets.CssSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "CssSize",
                Expected: FormatSize(budgets.CssSize.Value),
                Actual: FormatSize(cssSize),
                Severity: budgets.Action,
                Message: $"CSS size {FormatSize(cssSize)} exceeds budget of {FormatSize(budgets.CssSize.Value)}."
            ));
        }

        if (budgets.ImageSize.HasValue && imageSize > budgets.ImageSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "ImageSize",
                Expected: FormatSize(budgets.ImageSize.Value),
                Actual: FormatSize(imageSize),
                Severity: budgets.Action,
                Message: $"Image size {FormatSize(imageSize)} exceeds budget of {FormatSize(budgets.ImageSize.Value)}."
            ));
        }

        if (budgets.JavaScriptSize.HasValue && jsSize > budgets.JavaScriptSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "JavaScriptSize",
                Expected: FormatSize(budgets.JavaScriptSize.Value),
                Actual: FormatSize(jsSize),
                Severity: budgets.Action,
                Message: $"JavaScript size {FormatSize(jsSize)} exceeds budget of {FormatSize(budgets.JavaScriptSize.Value)}."
            ));
        }

        if (budgets.CssSize.HasValue && cssSize > budgets.CssSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "CssSize",
                Expected: FormatSize(budgets.CssSize.Value),
                Actual: FormatSize(cssSize),
                Severity: budgets.Action,
                Message: $"CSS size {FormatSize(cssSize)} exceeds budget of {FormatSize(budgets.CssSize.Value)}."
            ));
        }

        if (budgets.ImageSize.HasValue && imageSize > budgets.ImageSize.Value)
        {
            violations.Add(new BudgetViolation(
                Metric: "ImageSize",
                Expected: FormatSize(budgets.ImageSize.Value),
                Actual: FormatSize(imageSize),
                Severity: budgets.Action,
                Message: $"Image size {FormatSize(imageSize)} exceeds budget of {FormatSize(budgets.ImageSize.Value)}."
            ));
        }

        report.Violations = violations;
        report.Passed = violations.Count == 0 || violations.All(v => v.Severity == BudgetViolationAction.LogOnly);

        return report;
    }

    private static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            _ => $"{bytes / (1024.0 * 1024.0):F1} MB",
        };
    }

    private async IAsyncEnumerable<string> EnumerateFilesAsync(string directory)
    {
        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            yield return file;
            await Task.CompletedTask;
        }
    }
}
