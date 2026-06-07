namespace NextNet.TemplatePackages;

/// <summary>
/// Metadata describing a cached template package.
/// Tracks identity, origin, integrity checksum, and storage location.
/// </summary>
public sealed record PackageMetadata
{
    /// <summary>Display name of the template package.</summary>
    public string Name { get; init; } = "";

    /// <summary>Semantic version string (e.g. "1.2.3").</summary>
    public string Version { get; init; } = "";

    /// <summary>Author or publisher of the template package.</summary>
    public string Author { get; init; } = "";

    /// <summary>
    /// SHA-256 hex digest of the package file contents.
    /// Used to verify integrity after download and before extraction.
    /// </summary>
    public string ChecksumSha256 { get; init; } = "";

    /// <summary>File size of the cached package in bytes.</summary>
    public long SizeBytes { get; init; }

    /// <summary>Timestamp when the package was downloaded and cached.</summary>
    public DateTime DownloadedAt { get; init; }

    /// <summary>Absolute path to the cached .nntemplate file on disk.</summary>
    public string FilePath { get; init; } = "";
}
