namespace NextNet.TemplateMarketplace;

/// <summary>
/// Template discovery service for searching and browsing templates.
/// V4+ marketplace UI will implement this with ranking and filtering.
/// </summary>
public interface ITemplateDiscovery
{
    /// <summary>Searches templates by query string.</summary>
    Task<SearchRanking> SearchAsync(string query, CancellationToken ct = default);

    /// <summary>Gets the top-rated templates.</summary>
    Task<IReadOnlyList<TemplateRating>> GetTopRatedAsync(int count = 10, CancellationToken ct = default);

    /// <summary>Gets trending templates based on recent activity.</summary>
    Task<IReadOnlyList<TemplateRating>> GetTrendingAsync(int count = 10, CancellationToken ct = default);
}
