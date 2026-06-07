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
public class BudgetViolation
{
    /// <summary>
    /// The metric that was violated (e.g., "TotalSize", "JavaScriptSize").
    /// </summary>
    public string Metric { get; set; } = string.Empty;

    /// <summary>
    /// The expected (maximum) value.
    /// </summary>
    public string Expected { get; set; } = string.Empty;

    /// <summary>
    /// The actual value measured.
    /// </summary>
    public string Actual { get; set; } = string.Empty;

    /// <summary>
    /// The severity of the violation.
    /// </summary>
    public BudgetViolationAction Severity { get; set; }

    /// <summary>
    /// Human-readable message describing the violation.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
