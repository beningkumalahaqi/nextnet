namespace NextNet.TemplateRegistry;

/// <summary>
/// Detailed version information for a specific release of a template.
/// </summary>
public sealed record TemplateVersionInfo
{
    /// <summary>
    /// The semantic version string for this release.
    /// </summary>
    public string Version { get; init; } = "";

    /// <summary>
    /// The date and time this version was published.
    /// </summary>
    public DateTime PublishedAt { get; init; }

    /// <summary>
    /// The minimum NextNet framework version required by this template, if specified.
    /// </summary>
    public string? MinNextNetVersion { get; init; }

    /// <summary>
    /// The SHA-256 checksum of the template package, for integrity verification.
    /// </summary>
    public string? ChecksumSha256 { get; init; }

    /// <summary>
    /// The size of the template package in bytes.
    /// </summary>
    public long SizeBytes { get; init; }

    /// <summary>
    /// The URL from which this version can be downloaded, if different from the default.
    /// </summary>
    public string? DownloadUrl { get; init; }
}
