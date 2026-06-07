namespace NextNet.IO;

/// <summary>
/// Default implementation of <see cref="ISharpFileSystem"/> using real <c>System.IO</c> APIs.
/// </summary>
public class DefaultSharpFileSystem : ISharpFileSystem
{
    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public string ReadAllText(string path) => File.ReadAllText(path);

    /// <inheritdoc />
    public Task<string> ReadAllTextAsync(string path) => File.ReadAllTextAsync(path);

    /// <inheritdoc />
    public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

    /// <inheritdoc />
    public Task WriteAllTextAsync(string path, string content) => File.WriteAllTextAsync(path, content);

    /// <inheritdoc />
    public IEnumerable<string> EnumerateFiles(string directory, string pattern)
        => Directory.EnumerateFiles(directory, pattern, SearchOption.TopDirectoryOnly);

    /// <inheritdoc />
    public IEnumerable<string> EnumerateDirectories(string directory)
        => Directory.EnumerateDirectories(directory);

    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public void CreateDirectory(string path) => Directory.CreateDirectory(path);

    /// <inheritdoc />
    public string GetFullPath(string path) => Path.GetFullPath(path);

    /// <inheritdoc />
    public string? GetDirectoryName(string? path) => Path.GetDirectoryName(path);

    /// <inheritdoc />
    public string GetFileName(string path) => Path.GetFileName(path);

    /// <inheritdoc />
    public string GetFileNameWithoutExtension(string path) => Path.GetFileNameWithoutExtension(path);

    /// <inheritdoc />
    public string Combine(params string[] paths) => Path.Combine(paths);

    /// <inheritdoc />
    public Task WriteAllBytesAsync(string path, byte[] content, CancellationToken cancellationToken = default)
        => File.WriteAllBytesAsync(path, content, cancellationToken);

    /// <inheritdoc />
    public void DeleteDirectory(string path, bool recursive = true)
        => Directory.Delete(path, recursive);
}
