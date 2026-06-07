namespace NextNet.Routing.Models;

/// <summary>
/// Describes the type of file system change detected by <see cref="RouteFileWatcher"/>.
/// </summary>
public enum FileChangeType
{
    /// <summary>
    /// A file was created.
    /// </summary>
    Created,

    /// <summary>
    /// A file was modified.
    /// </summary>
    Modified,

    /// <summary>
    /// A file was deleted.
    /// </summary>
    Deleted,
}

/// <summary>
/// Contains information about a file system change event raised by <see cref="RouteFileWatcher"/>.
/// </summary>
public class FileChangeEvent
{
    /// <summary>
    /// Gets the full path to the file that changed.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the type of change that occurred.
    /// </summary>
    public FileChangeType ChangeType { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="FileChangeEvent"/>.
    /// </summary>
    /// <param name="filePath">The full path to the file that changed.</param>
    /// <param name="changeType">The type of change.</param>
    public FileChangeEvent(string filePath, FileChangeType changeType)
    {
        FilePath = filePath;
        ChangeType = changeType;
    }

    /// <inheritdoc />
    public override string ToString()
        => $"[{ChangeType}] {FilePath}";
}
