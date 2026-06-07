namespace NextNet.TemplateRegistry;

/// <summary>
/// Represents a downloaded template package with its metadata and content stream.
/// </summary>
public sealed record TemplateDownloadInfo
{
    /// <summary>
    /// The name of the downloaded template.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// The version of the downloaded template.
    /// </summary>
    public string Version { get; init; } = "";

    /// <summary>
    /// The SHA-256 checksum of the package content for integrity verification.
    /// </summary>
    public string ChecksumSha256 { get; init; } = "";

    /// <summary>
    /// The total size of the package content in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// The stream containing the downloaded template package content.
    /// </summary>
    public Stream Content { get; init; } = Stream.Null;
}
