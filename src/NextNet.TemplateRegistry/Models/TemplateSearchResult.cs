namespace NextNet.TemplateRegistry;

/// <summary>
/// A paginated set of template search results returned by the registry API.
/// </summary>
public sealed record TemplateSearchResult
{
    /// <summary>
    /// The template metadata items on the current page.
    /// </summary>
    public IReadOnlyList<TemplateMetadata> Items { get; init; } = Array.Empty<TemplateMetadata>();

    /// <summary>
    /// The total number of matching templates across all pages.
    /// </summary>
    public int TotalCount { get; init; }

    /// <summary>
    /// The current page index (1-based).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; init; }
}
