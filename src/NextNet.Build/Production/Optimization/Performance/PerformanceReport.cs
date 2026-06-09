namespace NextNet.Build.Production.Optimization.Performance;

/// <summary>
/// Detailed performance report generated after evaluating budgets.
/// </summary>
public class PerformanceReport
{
    /// <summary>
    /// Whether all budgets passed.
    /// </summary>
    public bool Passed { get; set; }

    /// <summary>
    /// Total output size in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// JavaScript bundle size in bytes.
    /// </summary>
    public long JavaScriptSize { get; set; }

    /// <summary>
    /// CSS size in bytes.
    /// </summary>
    public long CssSize { get; set; }

    /// <summary>
    /// Total image size in bytes.
    /// </summary>
    public long ImageSize { get; set; }

    /// <summary>
    /// Total number of HTTP requests for assets.
    /// </summary>
    public int RequestCount { get; set; }

    /// <summary>
    /// List of budget violations.
    /// </summary>
    public List<BudgetViolation> Violations { get; set; } = new();
}

/// <summary>
/// Represents a single budget violation.
/// </summary>
/// <param name="Metric">The metric that was violated (e.g., "TotalSize", "JavaScriptSize").</param>
/// <param name="Expected">The expected (maximum) value.</param>
/// <param name="Actual">The actual value measured.</param>
/// <param name="Severity">The severity of the violation.</param>
/// <param name="Message">Human-readable message describing the violation.</param>
public sealed record BudgetViolation(
    string Metric,
    string Expected,
    string Actual,
    BudgetViolationAction Severity,
    string Message);
