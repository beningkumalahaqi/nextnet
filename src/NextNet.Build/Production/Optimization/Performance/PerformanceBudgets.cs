namespace NextNet.Build.Production.Optimization.Performance;

/// <summary>
/// Defines performance budgets that the build output must respect.
/// When any budget is exceeded, the configured <see cref="Action"/> is taken.
/// </summary>
public class PerformanceBudgets
{
    /// <summary>
    /// Maximum total output size in bytes.
    /// </summary>
    public long? TotalSize { get; set; }

    /// <summary>
    /// Maximum JavaScript bundle size in bytes.
    /// </summary>
    public long? JavaScriptSize { get; set; }

    /// <summary>
    /// Maximum CSS size in bytes.
    /// </summary>
    public long? CssSize { get; set; }

    /// <summary>
    /// Maximum image size in bytes.
    /// </summary>
    public long? ImageSize { get; set; }

    /// <summary>
    /// Maximum first byte time in milliseconds.
    /// </summary>
    public int? FirstByteTime { get; set; }

    /// <summary>
    /// Maximum Largest Contentful Paint in milliseconds.
    /// </summary>
    public int? LargestContentfulPaint { get; set; }

    /// <summary>
    /// Maximum Cumulative Layout Shift score.
    /// </summary>
    public double? CumulativeLayoutShift { get; set; }

    /// <summary>
    /// The action to take when a budget is violated.
    /// </summary>
    public BudgetViolationAction Action { get; set; } = BudgetViolationAction.Warn;
}
