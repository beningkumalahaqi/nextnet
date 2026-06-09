namespace NextNet.Build.Production.Optimization.CriticalCss;

/// <summary>
/// The result of critical CSS extraction.
/// </summary>
/// <param name="CriticalCss">The critical (above-the-fold) CSS to be inlined.</param>
/// <param name="FullCss">The full original CSS that should be deferred.</param>
/// <param name="ModifiedHtml">The HTML with critical CSS inlined and full CSS deferred.</param>
/// <param name="CriticalRuleCount">The number of CSS rules extracted as critical.</param>
/// <param name="TotalRuleCount">The total number of CSS rules.</param>
/// <param name="CriticalSizeBytes">The size of critical CSS in bytes.</param>
/// <param name="FullSizeBytes">The size of the full CSS in bytes.</param>
public sealed record CriticalCssResult(
    string CriticalCss,
    string FullCss,
    string ModifiedHtml,
    int CriticalRuleCount,
    int TotalRuleCount,
    int CriticalSizeBytes,
    int FullSizeBytes)
{
    /// <summary>
    /// Creates a new critical CSS result with default (empty) values.
    /// </summary>
    public CriticalCssResult()
        : this(string.Empty, string.Empty, string.Empty, 0, 0, 0, 0) { }
}
