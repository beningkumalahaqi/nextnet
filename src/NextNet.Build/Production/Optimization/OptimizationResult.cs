using NextNet.Build.Production.Build;

namespace NextNet.Build.Production.Optimization;

/// <summary>
/// The result of running the full optimization pipeline.
/// </summary>
public class OptimizationResult
{
    /// <summary>
    /// Whether all optimization passes completed successfully.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Metrics collected during optimization.
    /// </summary>
    public BuildMetrics? Metrics { get; set; }

    /// <summary>
    /// Warnings generated during optimization.
    /// </summary>
    public IReadOnlyList<string> Warnings { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Errors generated during optimization.
    /// </summary>
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();
}
