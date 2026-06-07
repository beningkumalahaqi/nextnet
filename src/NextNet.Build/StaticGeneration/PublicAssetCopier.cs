using NextNet.IO;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Copies static assets from the <c>public/</c> directory to the output directory
/// during static site generation.
/// </summary>
public class PublicAssetCopier
{
    private readonly string _publicDirectory;
    private readonly string _outputDirectory;
    private readonly ISharpFileSystem _fileSystem;
    private int _copiedFileCount;

    /// <summary>
    /// Initializes a new instance of <see cref="PublicAssetCopier"/>.
    /// </summary>
    /// <param name="publicDirectory">Absolute path to the public assets directory (e.g. <c>public</c>).</param>
    /// <param name="outputDirectory">Absolute path to the output directory (e.g. <c>dist</c>).</param>
    /// <param name="fileSystem">Optional file system abstraction. Defaults to <see cref="DefaultSharpFileSystem"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="publicDirectory"/> or <paramref name="outputDirectory"/> is null.</exception>
    public PublicAssetCopier(
        string publicDirectory,
        string outputDirectory,
        ISharpFileSystem? fileSystem = null)
    {
        _publicDirectory = publicDirectory ?? throw new ArgumentNullException(nameof(publicDirectory));
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
    }

    /// <summary>
    /// Gets the number of files copied in the last operation.
    /// </summary>
    public int CopiedFileCount => _copiedFileCount;

    /// <summary>
    /// Copies all files from the public directory to the output directory,
    /// preserving the directory structure.
    /// </summary>
    /// <returns>A list of relative paths of the copied files.</returns>
    public async Task<IReadOnlyList<string>> CopyAsync()
    {
        var copiedFiles = new List<string>();

        if (!_fileSystem.DirectoryExists(_publicDirectory))
        {
            _copiedFileCount = 0;
            return copiedFiles;
        }

        // Ensure output directory exists
        _fileSystem.CreateDirectory(_outputDirectory);

        await CopyDirectoryRecursiveAsync(_publicDirectory, _outputDirectory, copiedFiles);

        _copiedFileCount = copiedFiles.Count;
        return copiedFiles;
    }

    /// <summary>
    /// Recursively copies files from source to destination, preserving structure.
    /// </summary>
    private async Task CopyDirectoryRecursiveAsync(
        string sourceDir,
        string destDir,
        List<string> copiedFiles)
    {
        foreach (var file in _fileSystem.EnumerateFiles(sourceDir, "*"))
        {
            // Relative from sourceDir for correct destination path
            var relativeToSource = Path.GetRelativePath(sourceDir, file);
            var destFile = _fileSystem.Combine(destDir, relativeToSource);
            var destFileDir = _fileSystem.GetDirectoryName(destFile);

            if (destFileDir != null)
            {
                _fileSystem.CreateDirectory(destFileDir);
            }

            // Relative from _publicDirectory for the output list
            var relativeToRoot = Path.GetRelativePath(_publicDirectory, file)
                .Replace('\\', '/');

            // Use File.Copy directly since ISharpFileSystem doesn't have a Copy method
            File.Copy(file, destFile, overwrite: true);

            copiedFiles.Add(relativeToRoot);
        }

        foreach (var subDir in _fileSystem.EnumerateDirectories(sourceDir))
        {
            var subDirName = Path.GetFileName(subDir);
            var destSubDir = _fileSystem.Combine(destDir, subDirName);
            await CopyDirectoryRecursiveAsync(subDir, destSubDir, copiedFiles);
        }
    }
}
