namespace NextNet.Build.Production.Optimization.Performance;

/// <summary>
/// Defines the action to take when a performance budget is violated.
/// </summary>
public enum BudgetViolationAction
{
    /// <summary>
    /// Log a warning but continue the build.
    /// </summary>
    Warn,

    /// <summary>
    /// Fail the build with an error.
    /// </summary>
    Fail,

    /// <summary>
    /// Log a warning and generate a report entry.
    /// </summary>
    LogOnly,
}
