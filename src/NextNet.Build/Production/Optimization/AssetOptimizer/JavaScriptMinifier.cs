using System.Text.RegularExpressions;
using NextNet.IO;

namespace NextNet.Build.Production.Optimization.AssetOptimizer;

/// <summary>
/// Basic JavaScript minifier that removes whitespace, comments, and performs
/// simple size-reduction transformations.
/// </summary>
public sealed partial class JavaScriptMinifier : IAssetOptimizer
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="JavaScriptMinifier"/>.
    /// </summary>
    public JavaScriptMinifier(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension is ".js" or ".mjs" or ".cjs";

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
    /// Minifies the given JavaScript string.
    /// </summary>
    public static string Minify(string js)
    {
        if (string.IsNullOrEmpty(js))
            return js;

        var result = js;

        // Remove single-line comments (but not in strings/regex)
        result = SingleLineCommentPattern().Replace(result, string.Empty);

        // Remove multi-line comments
        result = MultiLineCommentPattern().Replace(result, string.Empty);

        // Remove whitespace around operators and braces
        result = WhitespacePattern().Replace(result, "$1$2$3");

        // Remove whitespace before commas
        result = BeforeCommaPattern().Replace(result, ",");

        // Remove whitespace after commas
        result = AfterCommaPattern().Replace(result, ",");

        // Collapse multiple spaces
        result = MultiSpacePattern().Replace(result, " ");

        // Remove unnecessary semicolons before closing braces
        result = LastSemicolonPattern().Replace(result, "}");

        // Shorten true/false
        result = result.Replace("!0", "!0"); // already short
        result = result.Replace("!1", "!1"); // already short

        return result.Trim();
    }

    [GeneratedRegex(@"//[^\n\r]*[\n\r]", RegexOptions.Compiled)]
    private static partial Regex SingleLineCommentPattern();

    [GeneratedRegex(@"/\*[\s\S]*?\*/", RegexOptions.Compiled)]
    private static partial Regex MultiLineCommentPattern();

    [GeneratedRegex(@"(\s*)([{}();,:+\-*/%&|^!<>=])(\s*)", RegexOptions.Compiled)]
    private static partial Regex WhitespacePattern();

    [GeneratedRegex(@"\s+,", RegexOptions.Compiled)]
    private static partial Regex BeforeCommaPattern();

    [GeneratedRegex(@",\s+", RegexOptions.Compiled)]
    private static partial Regex AfterCommaPattern();

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultiSpacePattern();

    [GeneratedRegex(@";\}", RegexOptions.Compiled)]
    private static partial Regex LastSemicolonPattern();
}
