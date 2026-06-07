namespace NextNet.TemplateMarketplace;

/// <summary>
/// Provider interface for marketplace data sources.
/// V3 implementation uses HTTP against the marketplace API.
/// V4+ may add additional providers (desktop app, in-editor).
/// </summary>
public interface IMarketplaceProvider
{
    /// <summary>Gets a publisher's profile by ID.</summary>
    Task<PublisherProfile?> GetPublisherAsync(string publisherId, CancellationToken ct = default);

    /// <summary>Gets aggregate ratings for a template.</summary>
    Task<IReadOnlyList<TemplateRating>> GetRatingsAsync(string templateName, CancellationToken ct = default);

    /// <summary>Gets all available template categories.</summary>
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
}
