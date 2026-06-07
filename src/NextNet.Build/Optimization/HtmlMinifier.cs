using System.Text.RegularExpressions;

namespace NextNet.Build.Optimization;

/// <summary>
/// Performs lossless HTML minification by removing unnecessary whitespace,
/// comments, and other reducible markup elements.
/// </summary>
/// <remarks>
/// This is a lightweight, regex-based HTML minifier suitable for build-time use.
/// For more advanced minification (e.g. CSS/JS within HTML), consider integrating
/// a dedicated library like AngleSharp or HtmlAgilityPack.
/// </remarks>
public static partial class HtmlMinifier
{
    // Compiled regex patterns for performance

    [GeneratedRegex(@"<!--[\s\S]*?-->", RegexOptions.Compiled)]
    private static partial Regex HtmlCommentsPattern();

    [GeneratedRegex(@">\s+<", RegexOptions.Compiled)]
    private static partial Regex WhitespaceBetweenTagsPattern();

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultipleWhitespacePattern();

    [GeneratedRegex(@"\s+/>", RegexOptions.Compiled)]
    private static partial Regex WhitespaceBeforeSelfClosePattern();

    [GeneratedRegex(@"\s+=""", RegexOptions.Compiled)]
    private static partial Regex WhitespaceBeforeEmptyAttrPattern();

    [GeneratedRegex(@"(?<=\s)""(?=\s*(?:/>|>|\s))", RegexOptions.Compiled)]
    private static partial Regex RedundantQuotesPattern();

    /// <summary>
    /// Minifies the given HTML string by removing comments, reducing whitespace,
    /// and trimming unnecessary characters.
    /// </summary>
    /// <param name="html">The HTML content to minify.</param>
    /// <returns>The minified HTML string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is null.</exception>
    public static string Minify(string html)
    {
        if (html == null) throw new ArgumentNullException(nameof(html));
        if (html.Length == 0) return html;

        var result = html;

        // 1. Remove HTML comments
        result = HtmlCommentsPattern().Replace(result, string.Empty);

        // 2. Remove whitespace between tags (replace ">    <" with "><")
        result = WhitespaceBetweenTagsPattern().Replace(result, "><");

        // 3. Collapse multiple whitespace characters to single space
        result = MultipleWhitespacePattern().Replace(result, " ");

        // 4. Remove whitespace before self-closing tags
        result = WhitespaceBeforeSelfClosePattern().Replace(result, "/>");

        // 5. Remove whitespace before empty attribute quotes
        result = WhitespaceBeforeEmptyAttrPattern().Replace(result, "=\"");

        // 6. Trim leading/trailing whitespace from the document
        result = result.Trim();

        return result;
    }

    /// <summary>
    /// Gets the minification ratio as a percentage (100 = no reduction, 50 = half size).
    /// </summary>
    /// <param name="originalSize">The original size in bytes.</param>
    /// <param name="minifiedSize">The minified size in bytes.</param>
    /// <returns>The size ratio (original / minified * 100).</returns>
    public static double GetRatio(int originalSize, int minifiedSize)
    {
        if (originalSize <= 0) return 100;
        return Math.Round((double)minifiedSize / originalSize * 100, 1);
    }
}
