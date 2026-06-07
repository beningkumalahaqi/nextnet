using System.Text.RegularExpressions;
using NextNet.IO;

namespace NextNet.Build.Production.Optimization.AssetOptimizer;

/// <summary>
/// Optimizes SVG files by removing unnecessary metadata, whitespace, and comments.
/// </summary>
public partial class SvgOptimizer : IAssetOptimizer
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="SvgOptimizer"/>.
    /// </summary>
    public SvgOptimizer(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <inheritdoc />
    public bool CanHandle(string extension) =>
        extension.Equals(".svg", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<long> OptimizeAsync(string filePath)
    {
        if (!_fileSystem.FileExists(filePath))
            return 0;

        var original = new FileInfo(filePath).Length;
        var content = await _fileSystem.ReadAllTextAsync(filePath);
        var optimized = Optimize(content);

        if (optimized.Length >= content.Length)
            return 0;

        await _fileSystem.WriteAllTextAsync(filePath, optimized);
        return original - new FileInfo(filePath).Length;
    }

    /// <summary>
    /// Optimizes the given SVG string.
    /// </summary>
    public static string Optimize(string svg)
    {
        if (string.IsNullOrEmpty(svg))
            return svg;

        var result = svg;

        // Remove XML declaration
        result = XmlDeclarationPattern().Replace(result, string.Empty);

        // Remove comments
        result = CommentPattern().Replace(result, string.Empty);

        // Remove doctype
        result = DocTypePattern().Replace(result, string.Empty);

        // Remove whitespace between tags
        result = TagWhitespacePattern().Replace(result, "><");

        // Collapse whitespace inside attributes
        result = AttributeWhitespacePattern().Replace(result, " ");

        // Remove unnecessary quotes around attribute values (where safe)
        // Only remove quotes around values that don't contain spaces or special chars
        result = AttributeQuotesPattern().Replace(result, "$1=$2");

        // Remove empty attributes
        result = EmptyAttributePattern().Replace(result, string.Empty);

        // Remove unnecessary namespace declarations
        result = SvgNsPattern().Replace(result, "<svg");

        // Remove default namespace on SVG
        result = DefaultNsPattern().Replace(result, "<svg ");

        // Collapse multiple spaces
        result = MultiSpacePattern().Replace(result, " ");

        // Remove trailing whitespace from style/script content
        result = StyleScriptTrimPattern().Replace(result, m =>
        {
            var tag = m.Groups[1].Value;
            var inner = m.Groups[2].Value.Trim();
            var end = m.Groups[3].Value;
            return $"<{tag}>{inner}{end}";
        });

        return result.Trim();
    }

    [GeneratedRegex(@"<\?xml[^>]*\?>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex XmlDeclarationPattern();

    [GeneratedRegex(@"<!--[\s\S]*?-->", RegexOptions.Compiled)]
    private static partial Regex CommentPattern();

    [GeneratedRegex(@"<!DOCTYPE[^>]*>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DocTypePattern();

    [GeneratedRegex(@">\s+<", RegexOptions.Compiled)]
    private static partial Regex TagWhitespacePattern();

    [GeneratedRegex(@"\s{2,}", RegexOptions.Compiled)]
    private static partial Regex MultiSpacePattern();

    [GeneratedRegex(@"\s+(?=[\w-]+=)", RegexOptions.Compiled)]
    private static partial Regex AttributeWhitespacePattern();

    // Match attributes where quotes can be safely removed
    [GeneratedRegex(@"\s+([\w-]+)=""([^""\s>]+)""", RegexOptions.Compiled)]
    private static partial Regex AttributeQuotesPattern();

    // Match empty attributes like attr=""
    [GeneratedRegex(@"\s+[\w-]+=""""", RegexOptions.Compiled)]
    private static partial Regex EmptyAttributePattern();

    // xmlns="http://www.w3.org/2000/svg" → remove if it's the default
    [GeneratedRegex(@"\s+xmlns=""http://www\.w3\.org/2000/svg""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SvgNsPattern();

    // Remaining xmlns attributes
    [GeneratedRegex(@"\s+xmlns:(\w+)=""[^""]+""", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DefaultNsPattern();

    // Trim whitespace in <style> and <script> content
    [GeneratedRegex(@"<(style|script)\b[^>]*>([\s\S]*?)</\1>", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex StyleScriptTrimPattern();
}
