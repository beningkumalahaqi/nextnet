using System.Text.RegularExpressions;

namespace NextNet.Build.Production.Optimization.CriticalCss;

/// <summary>
/// Extracts critical CSS by analyzing selectors that appear above the fold.
/// Uses a heuristic approach based on common above-the-fold selectors.
/// For a production implementation, consider using a headless browser like Playwright.
/// </summary>
/// <example>
/// <code>
/// ICriticalCssExtractor extractor = new CriticalCssExtractor();
/// var result = await extractor.ExtractAsync(htmlContent);
/// // result.ModifiedHtml contains inlined critical CSS + deferred full CSS
/// </code>
/// </example>
public sealed partial class CriticalCssExtractor : ICriticalCssExtractor
{
    // Common above-the-fold selectors (heuristic)
    private static readonly HashSet<string> CriticalSelectors = new(StringComparer.OrdinalIgnoreCase)
    {
        "html", "body", "header", "nav", "main", "hero", "banner",
        ".header", ".nav", ".hero", ".banner", ".logo", ".menu",
        "#header", "#nav", "#hero", "#banner", "#logo",
        "h1", "h2", "h3", "p", "a", "button", "img",
        ".container", ".wrapper", ".section-hero",
    };

    /// <inheritdoc />
    public Task<CriticalCssResult> ExtractAsync(string html, int viewportWidth = 1920, int viewportHeight = 1080)
    {
        if (string.IsNullOrEmpty(html))
            throw new ArgumentException("HTML content is required.", nameof(html));

        // Find all <style> blocks and <link rel="stylesheet"> references
        var styleBlocks = ExtractStyleBlocks(html);
        var cssLinks = ExtractCssLinks(html);

        // For inline styles, extract critical rules
        var allCss = string.Join("\n", styleBlocks);
        var criticalCss = ExtractCriticalRules(allCss);

        var totalRuleCount = CountRules(allCss);
        var criticalRuleCount = CountRules(criticalCss);

        // Generate modified HTML with critical CSS inlined and full CSS deferred
        var modifiedHtml = GenerateModifiedHtml(html, criticalCss, cssLinks);

        var result = new CriticalCssResult(
            CriticalCss: criticalCss,
            FullCss: allCss,
            ModifiedHtml: modifiedHtml,
            CriticalRuleCount: criticalRuleCount,
            TotalRuleCount: totalRuleCount,
            CriticalSizeBytes: criticalCss.Length,
            FullSizeBytes: allCss.Length
        );

        return Task.FromResult(result);
    }

    private static List<string> ExtractStyleBlocks(string html)
    {
        var blocks = new List<string>();
        var regex = StyleBlockPattern();
        var matches = regex.Matches(html);

        foreach (Match match in matches)
        {
            blocks.Add(match.Groups[1].Value);
        }

        return blocks;
    }

    private static List<string> ExtractCssLinks(string html)
    {
        var links = new List<string>();
        var regex = CssLinkPattern();
        var matches = regex.Matches(html);

        foreach (Match match in matches)
        {
            var href = match.Groups[1].Value;
            if (!string.IsNullOrEmpty(href))
                links.Add(href);
        }

        return links;
    }

    private static string ExtractCriticalRules(string css)
    {
        if (string.IsNullOrEmpty(css))
            return string.Empty;

        var criticalRules = new List<string>();
        var ruleRegex = RulePattern();

        foreach (Match match in ruleRegex.Matches(css))
        {
            var selectors = match.Groups[1].Value;

            // Check if any selector is considered critical
            if (IsSelectorCritical(selectors))
            {
                criticalRules.Add(match.Value);
            }
        }

        return string.Join("\n", criticalRules);
    }

    private static bool IsSelectorCritical(string selectors)
    {
        // Split compound selectors
        var parts = selectors.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();
            foreach (var critical in CriticalSelectors)
            {
                if (trimmed.Contains(critical, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            // Check for common critical patterns
            if (trimmed.StartsWith(".hero", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("#hero", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith(".banner", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("#banner", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith(".header", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("#header", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith(".logo", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("#logo", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static int CountRules(string css)
    {
        if (string.IsNullOrEmpty(css))
            return 0;

        return RulePattern().Matches(css).Count;
    }

    private static string GenerateModifiedHtml(string html, string criticalCss, List<string> cssLinks)
    {
        // Remove existing style blocks
        var modified = StyleBlockPattern().Replace(html, string.Empty);

        // Remove existing CSS link tags
        foreach (var link in cssLinks)
        {
            modified = CssLinkTagPattern().Replace(modified, string.Empty);
        }

        // Inject critical CSS inline in <head>
        var criticalStyleTag = $"<style id=\"critical-css\">{criticalCss}</style>";

        // Add deferred CSS loader with loadCSS polyfill
        var deferredLoader = $@"
<script>
(function() {{
    var cb = function() {{
        var l = document.createElement('link');
        l.rel = 'stylesheet';
        l.href = '{string.Join("', l.href = '", cssLinks)}';
        document.head.appendChild(l);
    }};
    if (window.requestAnimationFrame) {{
        window.requestAnimationFrame(cb);
    }} else {{
        window.addEventListener('load', cb);
    }}
}})();
</script>
<noscript>{string.Join("", cssLinks.Select(link => $"<link rel=\"stylesheet\" href=\"{link}\">"))}</noscript>";

        // Inject into <head>
        var headEndIndex = modified.IndexOf("</head>", StringComparison.OrdinalIgnoreCase);
        if (headEndIndex >= 0)
        {
            modified = modified.Insert(headEndIndex, criticalStyleTag + deferredLoader);
        }

        return modified;
    }

    [GeneratedRegex(@"<style[^>]*>([\s\S]*?)</style>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex StyleBlockPattern();

    [GeneratedRegex(@"<link[^>]*href=""([^""]*\.css)[^""]*""[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CssLinkPattern();

    [GeneratedRegex(@"<link[^>]*rel=""stylesheet""[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CssLinkTagPattern();

    [GeneratedRegex(@"([^{]+)\{([^}]+)\}", RegexOptions.Compiled)]
    private static partial Regex RulePattern();
}
