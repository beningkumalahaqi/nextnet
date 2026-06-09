using System.Text.RegularExpressions;
using NextNet.IO;

namespace NextNet.Build.Production.Optimization.AssetOptimizer;

/// <summary>
/// Minifies CSS files by removing whitespace, comments, and optimizing rules.
/// </summary>
public sealed partial class CssMinifier : IAssetOptimizer
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="CssMinifier"/>.
    /// </summary>
    public CssMinifier(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".css", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<long> OptimizeAsync(string filePath)
    {
        if (!_fileSystem.FileExists(filePath))
            return 0;

        var original = new FileInfo(filePath).Length;
        var content = await _fileSystem.ReadAllTextAsync(filePath);
        var minified = Minify(content);

        if (minified.Length >= content.Length)
            return 0;

        await _fileSystem.WriteAllTextAsync(filePath, minified);
        return original - new FileInfo(filePath).Length;
    }

    /// <summary>
    /// Minifies the given CSS string.
    /// </summary>
    public static string Minify(string css)
    {
        if (string.IsNullOrEmpty(css))
            return css;

        var result = css;

        // Remove comments
        result = CommentsPattern().Replace(result, string.Empty);

        // Remove whitespace around structural characters
        result = WhitespacePattern().Replace(result, "$1");

        // Remove leading/trailing whitespace from selectors and properties
        result = TrimPattern().Replace(result, "$1$2");

        // Remove unnecessary semicolons before closing braces
        result = LastSemicolonPattern().Replace(result, "}");

        // Collapse multiple spaces into one
        result = MultiSpacePattern().Replace(result, " ");

        // Remove spaces around operators and colons in property values
        result = ValueWhitespacePattern().Replace(result, "$1$2");

        // Remove units from zero values
        result = ZeroUnitPattern().Replace(result, "0");

        // Remove leading zeros from decimal values
        result = LeadingZeroPattern().Replace(result, ".$1");

        // Remove unnecessary quotes from URLs if possible
        result = UrlQuotesPattern().Replace(result, "url($1)");

        // Remove hex color shorthand where possible (6-digit -> 3-digit)
        result = HexColorPattern().Replace(result, m =>
        {
            var r = m.Groups[1].Value;
            var g = m.Groups[2].Value;
            var b = m.Groups[3].Value;
            if (r[0] == r[1] && g[0] == g[1] && b[0] == b[1])
                return $"#{r[0]}{g[0]}{b[0]}";
            return m.Value;
        });

        return result.Trim();
    }

    // CSS comment removal
    [GeneratedRegex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled)]
    private static partial Regex CommentsPattern();

    // Whitespace between selectors/values
    [GeneratedRegex(@"\s*([{};,:])\s*", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();

    // Trim whitespace around selectors and properties
    [GeneratedRegex(@"(^|[{;])\s+([.#@a-zA-Z_-])", RegexOptions.Compiled)]
    private static partial Regex TrimPattern();

    // Last semicolon before }
    [GeneratedRegex(@";\}", RegexOptions.Compiled)]
    private static partial Regex LastSemicolonPattern();

    // Multiple spaces
    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultiSpacePattern();

    // Whitespace in values
    [GeneratedRegex(@":\s+", RegexOptions.Compiled)]
    private static partial Regex ValueWhitespacePattern();

    // Zero units
    [GeneratedRegex(@"(?<=[^0-9])(0)(?:px|em|rem|vh|vw|pt|cm|mm|pc|ex|ch|vmin|vmax|%)\b", RegexOptions.Compiled)]
    private static partial Regex ZeroUnitPattern();

    // Leading zero removal
    [GeneratedRegex(@"(?<=[^0-9.])0+(\.\d+)", RegexOptions.Compiled)]
    private static partial Regex LeadingZeroPattern();

    // URL quotes removal
    [GeneratedRegex(@"url\(['""]([^'""\)]+)['""]\)", RegexOptions.Compiled)]
    private static partial Regex UrlQuotesPattern();

    // Hex color shorthand
    [GeneratedRegex(@"#([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})\b", RegexOptions.Compiled)]
    private static partial Regex HexColorPattern();
}
