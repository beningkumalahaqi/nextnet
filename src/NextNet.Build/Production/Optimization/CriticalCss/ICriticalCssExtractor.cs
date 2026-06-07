namespace NextNet.Build.Production.Optimization.CriticalCss;

/// <summary>
/// Extracts critical (above-the-fold) CSS from HTML content.
/// </summary>
public interface ICriticalCssExtractor
{
    /// <summary>
    /// Extracts the critical CSS from the given HTML content.
    /// </summary>
    /// <param name="html">The full HTML document.</param>
    /// <param name="viewportWidth">The viewport width to consider as "above the fold".</param>
    /// <param name="viewportHeight">The viewport height to consider as "above the fold".</param>
    /// <returns>A result containing the critical CSS and the deferred CSS.</returns>
    Task<CriticalCssResult> ExtractAsync(string html, int viewportWidth = 1920, int viewportHeight = 1080);
}
