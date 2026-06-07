namespace NextNet.TemplateRegistry;

/// <summary>
/// Public metadata about a template published on the registry.
/// </summary>
public sealed record TemplateMetadata
{
    /// <summary>
    /// The unique name of the template.
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// The author or organization that published the template.
    /// </summary>
    public string Author { get; init; } = "";

    /// <summary>
    /// A human-readable description of what the template generates.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// The latest published semantic version string.
    /// </summary>
    public string LatestVersion { get; init; } = "";

    /// <summary>
    /// Optional tags for searching and categorizing the template.
    /// </summary>
    public IReadOnlyList<string>? Tags { get; init; }

    /// <summary>
    /// The date and time the template was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// The total number of times this template has been downloaded.
    /// </summary>
    public long DownloadCount { get; init; }
}
