namespace NextNet.Cli.Community;

/// <summary>
/// Represents a community template that has been installed locally.
/// </summary>
/// <remarks>
/// <para>
/// Each installed template is tracked in the <see cref="InstalledTemplateRegistry"/>
/// manifest at <c>~/.nextnet/templates/manifest.json</c>. The <see cref="Name"/>
/// is the primary key and must be unique across all installed templates.
/// </para>
/// <para>
/// The <see cref="ChecksumSha256"/> is optional and is populated when the
/// template package includes integrity verification (planned for a future phase).
/// </para>
/// </remarks>
public sealed record InstalledTemplate
{
    /// <summary>
    /// Gets the unique name of the template (e.g., "nextnet-webapi").
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the author or organization that created the template.
    /// </summary>
    public string Author { get; init; } = "";

    /// <summary>
    /// Gets the installed semantic version string (e.g., "1.2.3").
    /// </summary>
    public string Version { get; init; } = "";

    /// <summary>
    /// Gets the absolute filesystem path where the template is installed.
    /// </summary>
    public string InstallPath { get; init; } = "";

    /// <summary>
    /// Gets the UTC timestamp when the template was installed.
    /// </summary>
    public DateTime InstalledAt { get; init; }

    /// <summary>
    /// Gets the SHA-256 checksum of the template package for integrity verification,
    /// or <c>null</c> if not available.
    /// </summary>
    public string? ChecksumSha256 { get; init; }
}
