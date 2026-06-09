using NextNet.IO;
using NextNet.Build.Errors;

namespace NextNet.Build.StaticGeneration;

/// <summary>
/// Writes rendered HTML content to the output directory with proper
/// directory structure matching the route hierarchy.
/// </summary>
/// <remarks>
/// Route-to-file mapping:
/// <list type="bullet">
///   <item><c>/</c> → <c>index.html</c></item>
///   <item><c>/about</c> → <c>about/index.html</c></item>
///   <item><c>/blog/hello-world</c> → <c>blog/hello-world/index.html</c></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var writer = new OutputWriter(outputDir);
/// await writer.WriteAsync("/about", "&lt;html&gt;&lt;body&gt;About&lt;/body&gt;&lt;/html&gt;");
/// await writer.WriteBytesAsync("about/index.html.gz", compressedBytes);
/// </code>
/// </example>
public sealed class OutputWriter
{
    private readonly string _outputDirectory;
    private readonly ISharpFileSystem _fileSystem;
    private long _totalBytesWritten;

    /// <summary>
    /// Initializes a new instance of <see cref="OutputWriter"/>.
    /// </summary>
    /// <param name="outputDirectory">Absolute path to the output directory (e.g. <c>dist</c>).</param>
    /// <param name="fileSystem">Optional file system abstraction. Defaults to <see cref="DefaultSharpFileSystem"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="outputDirectory"/> is null.</exception>
    public OutputWriter(string outputDirectory, ISharpFileSystem? fileSystem = null)
    {
        _outputDirectory = outputDirectory ?? throw new ArgumentNullException(nameof(outputDirectory));
        _fileSystem = fileSystem ?? new DefaultSharpFileSystem();
    }

    /// <summary>
    /// Gets the total number of bytes written by this writer since construction.
    /// </summary>
    public long TotalBytesWritten => _totalBytesWritten;

    /// <summary>
    /// Converts a route path to a relative file path within the output directory.
    /// </summary>
    /// <param name="route">The route path (e.g. <c>"/"</c>, <c>"/about"</c>, <c>"/blog/hello-world"</c>).</param>
    /// <returns>The relative file path (e.g. <c>"index.html"</c>, <c>"about/index.html"</c>).</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="route"/> is null or empty.</exception>
    public static string RouteToFilePath(string route)
    {
        if (string.IsNullOrEmpty(route))
            throw new ArgumentException("Route cannot be null or empty.", nameof(route));

        // Normalize: trim leading and trailing slashes
        var normalized = route.Trim('/');

        if (normalized.Length == 0)
            return "index.html";

        return $"{normalized}/index.html";
    }

    /// <summary>
    /// Writes the given HTML content to the appropriate file path for the route.
    /// Creates directories as needed.
    /// </summary>
    /// <param name="route">The route path (e.g. <c>"/"</c>, <c>"/about"</c>).</param>
    /// <param name="htmlContent">The HTML content to write.</param>
    /// <returns>The relative path of the written file.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="route"/> or <paramref name="htmlContent"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown with error code DS-203 when the write operation fails.</exception>
    public async Task<string> WriteAsync(string route, string htmlContent)
    {
        if (route == null) throw new ArgumentNullException(nameof(route));
        if (htmlContent == null) throw new ArgumentNullException(nameof(htmlContent));

        try
        {
            var relativePath = RouteToFilePath(route);
            var absolutePath = _fileSystem.Combine(_outputDirectory, relativePath);
            var directory = _fileSystem.GetDirectoryName(absolutePath);

            if (directory != null)
            {
                _fileSystem.CreateDirectory(directory);
            }

            await _fileSystem.WriteAllTextAsync(absolutePath, htmlContent);

            var bytes = System.Text.Encoding.UTF8.GetByteCount(htmlContent);
            Interlocked.Add(ref _totalBytesWritten, bytes);

            return relativePath;
        }
        catch (Exception ex) when (ex is not ArgumentNullException)
        {
            throw new InvalidOperationException(
                $"[{BuildErrorCodes.OutputWriteFailed}] Failed to write output for route '{route}' to '{_outputDirectory}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Writes the given byte content (e.g. gzipped HTML) to the output directory.
    /// </summary>
    /// <param name="relativePath">The relative path within the output directory (e.g. <c>"about/index.html.gz"</c>).</param>
    /// <param name="content">The byte content to write.</param>
    /// <returns>The relative path of the written file.</returns>
    public async Task<string> WriteBytesAsync(string relativePath, byte[] content)
    {
        if (relativePath == null) throw new ArgumentNullException(nameof(relativePath));
        if (content == null) throw new ArgumentNullException(nameof(content));

        var absolutePath = _fileSystem.Combine(_outputDirectory, relativePath);
        var directory = _fileSystem.GetDirectoryName(absolutePath);

        if (directory != null)
        {
            _fileSystem.CreateDirectory(directory);
        }

        await File.WriteAllBytesAsync(absolutePath, content);

        Interlocked.Add(ref _totalBytesWritten, content.Length);

        return relativePath;
    }

    /// <summary>
    /// Cleans the output directory by deleting it and recreating it.
    /// </summary>
    public void CleanOutputDirectory()
    {
        if (_fileSystem.DirectoryExists(_outputDirectory))
        {
            Directory.Delete(_outputDirectory, recursive: true);
        }

        _fileSystem.CreateDirectory(_outputDirectory);
        _totalBytesWritten = 0;
    }

    /// <summary>
    /// Gets the absolute output directory path.
    /// </summary>
    public string OutputDirectoryPath => _outputDirectory;
}
