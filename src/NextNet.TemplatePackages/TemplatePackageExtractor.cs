namespace NextNet.TemplatePackages;

using System.IO.Compression;
using System.Security.Cryptography;

/// <summary>
/// Extracts .nntemplate (ZIP) package archives to a target directory.
/// Provides path-traversal protection and optional SHA-256 checksum verification.
/// </summary>
public sealed class TemplatePackageExtractor
{
    /// <summary>
    /// Extracts the contents of a package stream into <paramref name="targetDir"/>.
    /// </summary>
    /// <param name="packageStream">A readable stream containing a ZIP archive.</param>
    /// <param name="targetDir">The directory to extract files into. Created if it does not exist.</param>
    /// <param name="expectedChecksum">
    /// Optional SHA-256 hex digest to verify against. When provided, the stream is
    /// hashed during extraction and an <see cref="InvalidDataException"/> is thrown on mismatch.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An <see cref="ExtractResult"/> describing what was extracted.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="packageStream"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="targetDir"/> is null or whitespace.</exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the checksum does not match <paramref name="expectedChecksum"/>,
    /// or when a path-traversal attempt is detected in the archive.
    /// </exception>
    public async Task<ExtractResult> ExtractAsync(
        Stream packageStream,
        string targetDir,
        string? expectedChecksum = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(packageStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetDir);

        Directory.CreateDirectory(targetDir);

        // When a checksum is expected, buffer the stream to a temp file first so
        // we can both verify the hash and then do a seekable extraction.
        if (expectedChecksum is not null)
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                await using (var fs = File.Create(tempPath))
                {
                    await packageStream.CopyToAsync(fs, cancellationToken);
                }

                var actualChecksum = await ComputeSha256Async(tempPath, cancellationToken);
                if (!string.Equals(actualChecksum, expectedChecksum, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidDataException(
                        $"Package checksum mismatch. Expected: {expectedChecksum}, Actual: {actualChecksum}");
                }

                await using var fileStream = File.OpenRead(tempPath);
                return await ExtractFromFileAsync(fileStream, targetDir, cancellationToken);
            }
            finally
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        // Without checksum verification, extract directly from the stream
        return await ExtractFromFileAsync(packageStream, targetDir, cancellationToken);
    }

    private static async Task<ExtractResult> ExtractFromFileAsync(
        Stream source,
        string targetDir,
        CancellationToken cancellationToken)
    {
        // Materialize the stream to memory if it does not support seeking
        // (ZipArchive requires a seekable stream).
        if (!source.CanSeek)
        {
            var ms = new MemoryStream();
            await source.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;
            source = ms;
        }

        var extractedFiles = new List<string>();

        using (var archive = new ZipArchive(source, ZipArchiveMode.Read))
        {
            foreach (var entry in archive.Entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Path traversal prevention
                var sanitizedPath = SanitizePath(entry.FullName);
                if (sanitizedPath is null)
                {
                    throw new InvalidDataException(
                        $"Package contains path traversal: {entry.FullName}");
                }

                var targetPath = Path.Combine(
                    targetDir,
                    sanitizedPath.Replace('/', Path.DirectorySeparatorChar));

                if (string.IsNullOrEmpty(entry.Name))
                {
                    // Directory entry
                    Directory.CreateDirectory(targetPath);
                }
                else
                {
                    var dir = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    await using (var entryStream = entry.Open())
                    await using (var fileStream = File.Create(targetPath))
                    {
                        await entryStream.CopyToAsync(fileStream, cancellationToken);
                    }

                    extractedFiles.Add(targetPath);
                }
            }
        }

        return new ExtractResult
        {
            TargetDirectory = targetDir,
            ExtractedFiles = extractedFiles,
            FileCount = extractedFiles.Count
        };
    }

    /// <summary>
    /// Sanitizes an archive entry path to prevent directory traversal attacks.
    /// Returns null if the path is invalid or malicious.
    /// </summary>
    private static string? SanitizePath(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        // Reject path traversal sequences and absolute paths
        if (path.Contains("..") || path.StartsWith('/') || path.Contains(':'))
        {
            return null;
        }

        return path.Replace('\\', '/');
    }

    private static async Task<string> ComputeSha256Async(string filePath, CancellationToken ct)
    {
        await using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

/// <summary>
/// Describes the result of a template package extraction operation.
/// </summary>
public sealed record ExtractResult
{
    /// <summary>The directory into which files were extracted.</summary>
    public string TargetDirectory { get; init; } = "";

    /// <summary>Full paths of all files that were extracted.</summary>
    public IReadOnlyList<string> ExtractedFiles { get; init; } = Array.Empty<string>();

    /// <summary>Total number of files extracted.</summary>
    public int FileCount { get; init; }
}
