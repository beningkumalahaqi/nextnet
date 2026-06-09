using NextNet.IO;

namespace NextNet.Build.Production.Optimization;

/// <summary>
/// Analyzes the build output directory to understand bundle composition,
/// identify large files, and generate treemap data for visualization.
/// </summary>
public sealed class BundleAnalyzer
{
    private readonly ISharpFileSystem _fileSystem;

    /// <summary>
    /// Initializes a new instance of <see cref="BundleAnalyzer"/>.
    /// </summary>
    public BundleAnalyzer(ISharpFileSystem fileSystem)
    {
        _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    }

    /// <summary>
    /// Analyzes the given output directory and returns bundle metrics.
    /// </summary>
    /// <param name="outputDirectory">The build output directory.</param>
    /// <returns>A detailed analysis result.</returns>
    public Task<BundleAnalysisResult> AnalyzeAsync(string outputDirectory)
    {
        if (string.IsNullOrEmpty(outputDirectory))
            throw new ArgumentException("Output directory is required.", nameof(outputDirectory));

        var result = new BundleAnalysisResult();
        var files = new List<FileEntry>();
        var byExtension = new Dictionary<string, FileTypeSummary>(StringComparer.OrdinalIgnoreCase);

        if (!_fileSystem.DirectoryExists(outputDirectory))
            return Task.FromResult(result);

        CollectFiles(outputDirectory, outputDirectory, files, byExtension);

        result.TotalFiles = files.Count;
        result.TotalSize = files.Sum(f => f.Size);
        result.ByExtension = byExtension;
        result.LargestFiles = files.OrderByDescending(f => f.Size).Take(20).ToList();
        result.Treemap = BuildTreemap(files);

        return Task.FromResult(result);
    }

    private void CollectFiles(string rootDir, string currentDir, List<FileEntry> files, Dictionary<string, FileTypeSummary> byExtension)
    {
        foreach (var file in Directory.EnumerateFiles(currentDir))
        {
            var fileInfo = new FileInfo(file);
            var ext = fileInfo.Extension;
            var relativePath = Path.GetRelativePath(rootDir, file);

            var entry = new FileEntry
            {
                Path = relativePath,
                Size = fileInfo.Length,
                Extension = ext,
            };

            files.Add(entry);

            if (!byExtension.ContainsKey(ext))
            {
                byExtension[ext] = new FileTypeSummary
                {
                    Extension = ext,
                };
            }

            byExtension[ext].Count++;
            byExtension[ext].TotalSize += fileInfo.Length;
            if (fileInfo.Length > byExtension[ext].LargestSize)
                byExtension[ext].LargestSize = fileInfo.Length;
        }

        foreach (var subDir in Directory.EnumerateDirectories(currentDir))
        {
            CollectFiles(rootDir, subDir, files, byExtension);
        }
    }

    private static List<TreemapItem> BuildTreemap(List<FileEntry> files)
    {
        // Build a simple directory tree grouped by first path segment
        var root = new Dictionary<string, TreemapItem>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var parts = file.Path.Split('/', '\\');
            var topDir = parts.Length > 1 ? parts[0] : "(root)";

            if (!root.ContainsKey(topDir))
            {
                root[topDir] = new TreemapItem
                {
                    Name = topDir,
                    IsDirectory = parts.Length > 1,
                    Size = 0,
                };
            }

            root[topDir].Size += file.Size;

            if (parts.Length <= 1)
            {
                root[topDir].Children ??= new List<TreemapItem>();
                root[topDir].Children.Add(new TreemapItem
                {
                    Name = file.Path,
                    Size = file.Size,
                });
            }
        }

        return root.Values.OrderByDescending(i => i.Size).ToList();
    }
}
