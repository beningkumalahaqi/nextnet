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
/// <param name="FilePath">The full path to the file that changed.</param>
/// <param name="ChangeType">The type of change that occurred.</param>
public sealed record FileChangeEvent(string FilePath, FileChangeType ChangeType)
{
    /// <inheritdoc />
    public override string ToString()
        => $"[{ChangeType}] {FilePath}";
}
