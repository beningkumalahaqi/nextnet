using NextNet.IO;

namespace NextNet.Build.Optimization;

/// <summary>
/// Applies optimisation passes to assets after they have been copied
/// to the output directory. Currently handles basic minification of
/// known text-based asset types.
/// </summary>
public sealed class AssetOptimizer
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// The set of file extensions that are eligible for optimisation.
    /// </summary>
    public static readonly HashSet<string> OptimizableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".html", ".css", ".js", ".svg", ".xml", ".json",
    };

    /// <summary>
    /// Initializes a new instance of <see cref="AssetOptimizer"/>.
    /// </summary>
    /// <param name="fileSystem">Optional file system abstraction.</param>
    public AssetOptimizer(ISharpFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
    }

    /// <summary>
    /// Optimizes all eligible assets in the given directory.
    /// </summary>
    /// <param name="directory">The directory containing assets to optimize.</param>
    /// <returns>The number of bytes saved.</returns>
    public async Task<long> OptimizeDirectoryAsync(string directory)
    {
        if (directory == null) throw new ArgumentNullException(nameof(directory));
        long totalBytesSaved = 0;

        if (!_fileSystem.DirectoryExists(directory))
            return 0;

        var files = new List<string>();
        CollectFilesRecursive(directory, files);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file);
            if (!OptimizableExtensions.Contains(extension))
                continue;

            var beforeSize = new FileInfo(file).Length;

            if (extension.Equals(".html", StringComparison.OrdinalIgnoreCase))
            {
                var content = await _fileSystem.ReadAllTextAsync(file);
                var minified = HtmlMinifier.Minify(content);
                await _fileSystem.WriteAllTextAsync(file, minified);
            }
            // For CSS, JS, SVG, XML, JSON — basic whitespace reduction
            else
            {
                var content = await _fileSystem.ReadAllTextAsync(file);
                var optimized = BasicWhitespaceReduce(content);
                if (optimized.Length < content.Length)
                {
                    await _fileSystem.WriteAllTextAsync(file, optimized);
                }
            }

            var afterSize = new FileInfo(file).Length;
            totalBytesSaved += beforeSize - afterSize;
        }

        return totalBytesSaved;
    }

    /// <summary>
    /// Collects all files recursively from a directory.
    /// </summary>
    private static void CollectFilesRecursive(string directory, List<string> files)
    {
        foreach (var file in Directory.EnumerateFiles(directory))
        {
            files.Add(file);
        }

        foreach (var subDir in Directory.EnumerateDirectories(directory))
        {
            CollectFilesRecursive(subDir, files);
        }
    }

    /// <summary>
    /// Basic whitespace reduction for text-based assets (CSS, JS, etc.).
    /// Collapses multiple whitespace characters into a single space.
    /// </summary>
    private static string BasicWhitespaceReduce(string content)
    {
        if (string.IsNullOrEmpty(content)) return content;

        var result = System.Text.RegularExpressions.Regex.Replace(content, @"\s+", " ");
        result = result.Trim();

        // Remove whitespace around certain characters for CSS/JS
        result = System.Text.RegularExpressions.Regex.Replace(result, @"\s*([{}();,:])\s*", "$1");

        return result;
    }
}
