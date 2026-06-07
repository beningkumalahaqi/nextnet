namespace NextNet.IO;

/// <summary>
/// Abstraction over the file system to enable testability and
/// decouple NextNet components from direct <c>System.IO</c> dependencies.
/// </summary>
public interface ISharpFileSystem
{
    /// <summary>
    /// Determines whether the specified file exists.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Reads all text from the specified file.
    /// </summary>
    string ReadAllText(string path);

    /// <summary>
    /// Reads all text from the specified file asynchronously.
    /// </summary>
    Task<string> ReadAllTextAsync(string path);

    /// <summary>
    /// Writes text to the specified file, overwriting it if it exists.
    /// </summary>
    void WriteAllText(string path, string content);

    /// <summary>
    /// Writes text to the specified file asynchronously, overwriting it if it exists.
    /// </summary>
    Task WriteAllTextAsync(string path, string content);

    /// <summary>
    /// Returns an enumerable collection of file names that match a search pattern
    /// in the specified directory.
    /// </summary>
    IEnumerable<string> EnumerateFiles(string directory, string pattern);

    /// <summary>
    /// Returns an enumerable collection of directory names in the specified directory.
    /// </summary>
    IEnumerable<string> EnumerateDirectories(string directory);

    /// <summary>
    /// Determines whether the specified directory exists.
    /// </summary>
    bool DirectoryExists(string path);

    /// <summary>
    /// Creates the specified directory and any missing parent directories.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Gets the absolute path for the specified path.
    /// </summary>
    string GetFullPath(string path);

    /// <summary>
    /// Gets the directory information for the specified path.
    /// </summary>
    string? GetDirectoryName(string? path);

    /// <summary>
    /// Writes the specified byte array to the file asynchronously, overwriting it if it exists.
    /// </summary>
    /// <param name="path">The file path to write to.</param>
    /// <param name="content">The byte content to write.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the specified directory and, if recursive, all subdirectories and files.
    /// </summary>
    /// <param name="path">The directory path to delete.</param>
    /// <param name="recursive">Whether to delete subdirectories and files recursively.</param>
    void DeleteDirectory(string path, bool recursive = true);

    /// <summary>
    /// Gets the file name of the specified path.
    /// </summary>
    string GetFileName(string path);

    /// <summary>
    /// Gets the file name without extension from the specified path.
    /// </summary>
    string GetFileNameWithoutExtension(string path);

    /// <summary>
    /// Combines an array of path segments into a single path.
    /// </summary>
    string Combine(params string[] paths);
}
