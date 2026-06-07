namespace NextNet.Build.Production.Optimization.CriticalCss;

/// <summary>
/// The result of critical CSS extraction.
/// </summary>
public class CriticalCssResult
{
    /// <summary>
    /// The critical (above-the-fold) CSS to be inlined.
    /// </summary>
    public string CriticalCss { get; set; } = string.Empty;

    /// <summary>
    /// The full original CSS that should be deferred.
    /// </summary>
    public string FullCss { get; set; } = string.Empty;

    /// <summary>
    /// The HTML with critical CSS inlined and full CSS deferred.
    /// </summary>
    public string ModifiedHtml { get; set; } = string.Empty;

    /// <summary>
    /// The number of CSS rules extracted as critical.
    /// </summary>
    public int CriticalRuleCount { get; set; }

    /// <summary>
    /// The total number of CSS rules.
    /// </summary>
    public int TotalRuleCount { get; set; }

    /// <summary>
    /// The size of critical CSS in bytes.
    /// </summary>
    public int CriticalSizeBytes { get; set; }

    /// <summary>
    /// The size of the full CSS in bytes.
    /// </summary>
    public int FullSizeBytes { get; set; }
}
