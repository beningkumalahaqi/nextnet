namespace NextNet.Build.Production.Optimization;

/// <summary>
/// The result of analyzing the build output bundles.
/// </summary>
public class BundleAnalysisResult
{
    /// <summary>
    /// Total size of all output in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Total number of files.
    /// </summary>
    public int TotalFiles { get; set; }

    /// <summary>
    /// Breakdown of file sizes by extension.
    /// </summary>
    public Dictionary<string, FileTypeSummary> ByExtension { get; set; } = new();

    /// <summary>
    /// The largest files in the output.
    /// </summary>
    public List<FileEntry> LargestFiles { get; set; } = new();

    /// <summary>
    /// A treemap representation for visualization (flat list of rects).
    /// </summary>
    public List<TreemapItem> Treemap { get; set; } = new();
}

/// <summary>
/// Summary of files for a given extension type.
/// </summary>
public class FileTypeSummary
{
    /// <summary>
    /// The file extension (e.g., ".js", ".css").
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// Total count of files with this extension.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Total size in bytes.
    /// </summary>
    public long TotalSize { get; set; }

    /// <summary>
    /// Size of the largest single file.
    /// </summary>
    public long LargestSize { get; set; }
}

/// <summary>
/// A file entry in the bundle analysis.
/// </summary>
public class FileEntry
{
    /// <summary>
    /// The relative path within the output directory.
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// File extension.
    /// </summary>
    public string Extension { get; set; } = string.Empty;
}

/// <summary>
/// An item in the treemap visualization, representing a file or directory.
/// </summary>
public class TreemapItem
{
    /// <summary>
    /// Display name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Whether this item is a directory.
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// Child items (for directories).
    /// </summary>
    public List<TreemapItem> Children { get; set; } = new();
}
