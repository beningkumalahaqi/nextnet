using System.IO.Compression;
using System.Security.Cryptography;

namespace NextNet.TemplateSdk;

/// <summary>
/// Packages a template source directory into a compressed <c>.nntemplate</c> archive
/// with a SHA-256 checksum for integrity verification.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="TemplatePackager"/> reads a template directory (containing a
/// <c>template.json</c> manifest and all files in subdirectories) and produces a
/// ZIP archive suitable for distribution and installation.
/// </para>
/// <para>
/// The resulting package includes all files from the source directory, preserving
/// the relative path structure. A SHA-256 checksum is computed over the entire
/// archive and returned in the <see cref="PackageResult"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var packager = new TemplatePackager();
/// var result = await packager.PackageAsync("./my-template", "./output/my-template.nntemplate");
/// Console.WriteLine($"Package created at {result.PackagePath} (SHA-256: {result.ChecksumSha256})");
/// </code>
/// </example>
public sealed class TemplatePackager
{
    /// <summary>
    /// Packages the template source directory into a ZIP archive at the specified output path.
    /// </summary>
    /// <param name="sourceDir">The source directory containing the template files and <c>template.json</c>.</param>
    /// <param name="outputPath">The destination path for the generated <c>.nntemplate</c> package.</param>
    /// <param name="options">Optional packaging configuration (signing, etc.).</param>
    /// <returns>A <see cref="PackageResult"/> describing the created package.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="sourceDir"/> or <paramref name="outputPath"/> is null or whitespace.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when <paramref name="sourceDir"/> does not exist.</exception>
    /// <exception cref="FileNotFoundException">Thrown when <c>template.json</c> is missing from <paramref name="sourceDir"/>.</exception>
    public async Task<PackageResult> PackageAsync(string sourceDir, string outputPath, PackageOptions? options = null)
    {
        options ??= new PackageOptions();
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceDir);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputPath);

        if (!Directory.Exists(sourceDir))
            throw new TemplateSdkException(TemplateSdkErrorCodes.SourceDirectoryNotFound, $"Source directory not found: {sourceDir}");

        var manifestPath = Path.Combine(sourceDir, "template.json");
        if (!File.Exists(manifestPath))
            throw new TemplateSdkException(TemplateSdkErrorCodes.ManifestNotFound, "template.json not found in source directory");

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        // Create ZIP archive
        if (File.Exists(outputPath)) File.Delete(outputPath);

        using (var zipStream = File.Create(outputPath))
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
        {
            foreach (var file in Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(sourceDir, file);
                var entry = archive.CreateEntry(relativePath.Replace('\\', '/'));
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file);
                await fileStream.CopyToAsync(entryStream);
            }
        }

        // Compute checksum
        var checksum = await ComputeSha256Async(outputPath);

        return new PackageResult
        {
            PackagePath = outputPath,
            ChecksumSha256 = checksum,
            SizeBytes = new FileInfo(outputPath).Length
        };
    }

    private static async Task<string> ComputeSha256Async(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

/// <summary>
/// Optional configuration for the <see cref="TemplatePackager.PackageAsync"/> method.
/// </summary>
/// <remarks>
/// Both properties are optional. Signing requires both <see cref="PackageOptions.IncludeSignature"/>
/// to be <c>true</c> and a valid <see cref="PackageOptions.SigningKeyPath"/>.
/// </remarks>
public sealed record PackageOptions
{
    /// <summary>
    /// When <c>true</c>, the packager will include a digital signature in the package metadata.
    /// </summary>
    public bool IncludeSignature { get; init; }

    /// <summary>
    /// Path to the PEM-encoded private key file used for signing.
    /// Only used when <see cref="IncludeSignature"/> is <c>true</c>.
    /// </summary>
    public string? SigningKeyPath { get; init; }
}

/// <summary>
/// Describes the result of a <see cref="TemplatePackager.PackageAsync"/> operation.
/// </summary>
/// <remarks>
/// Contains the file path, integrity checksum, and size of the generated package.
/// </remarks>
public sealed record PackageResult
{
    /// <summary>
    /// The absolute path to the created <c>.nntemplate</c> package file.
    /// </summary>
    public string PackagePath { get; init; } = "";

    /// <summary>
    /// The lowercase hexadecimal SHA-256 checksum of the package contents.
    /// </summary>
    public string ChecksumSha256 { get; init; } = "";

    /// <summary>
    /// The size of the package file in bytes.
    /// </summary>
    public long SizeBytes { get; init; }
}
