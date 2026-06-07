namespace NextNet.TemplateMarketplace;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

/// <summary>
/// Extension methods for registering NextNet Template Marketplace services with DI.
/// </summary>
public static class MarketplaceServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet Template Marketplace services to the service collection.
    /// Registers <see cref="MarketplaceApiClient"/>, <see cref="MarketplaceCache"/>,
    /// <see cref="MarketplaceDataCollector"/>, <see cref="IMarketplaceProvider"/>,
    /// and <see cref="ITemplateDiscovery"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration delegate for <see cref="MarketplaceOptions"/>.</param>
    public static IServiceCollection AddNextNetTemplateMarketplace(
        this IServiceCollection services,
        Action<MarketplaceOptions>? configure = null)
    {
        var options = new MarketplaceOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddHttpClient<MarketplaceApiClient>();
        services.AddSingleton<MarketplaceCache>();
        services.AddSingleton<MarketplaceDataCollector>();
        services.AddSingleton<IMarketplaceProvider, MarketplaceProviderAdapter>();
        services.AddSingleton<ITemplateDiscovery, TemplateDiscoveryAdapter>();

        return services;
    }
}

/// <summary>
/// Adapter from <see cref="IMarketplaceProvider"/> to <see cref="MarketplaceApiClient"/>.
/// </summary>
internal sealed class MarketplaceProviderAdapter : IMarketplaceProvider
{
    private readonly MarketplaceApiClient _client;

    public MarketplaceProviderAdapter(MarketplaceApiClient client) => _client = client;

    public Task<PublisherProfile?> GetPublisherAsync(string publisherId, CancellationToken ct = default)
        => _client.GetPublisherProfileAsync(publisherId, ct);

    public async Task<IReadOnlyList<TemplateRating>> GetRatingsAsync(string templateName, CancellationToken ct = default)
    {
        var list = await _client.GetRatingsAsync(templateName, ct);
        return list;
    }

    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
        => _client.GetCategoriesAsync(ct);
}

/// <summary>
/// Adapter from <see cref="ITemplateDiscovery"/> to <see cref="MarketplaceApiClient"/>.
/// V3 provides a simple implementation; V4+ will add ranking algorithms.
/// </summary>
internal sealed class TemplateDiscoveryAdapter : ITemplateDiscovery
{
    private readonly MarketplaceApiClient _client;

    public TemplateDiscoveryAdapter(MarketplaceApiClient client) => _client = client;

    public async Task<SearchRanking> SearchAsync(string query, CancellationToken ct = default)
    {
        try
        {
            var ratings = await _client.GetRatingsAsync(query, ct);
            var results = ratings.Select(r => new RankingResult
            {
                TemplateName = r.TemplateName,
                Score = r.AverageRating,
                AverageRating = r.AverageRating,
                Downloads = 0
            }).ToList();

            return new SearchRanking
            {
                Query = query,
                Results = results,
                ComputedAt = DateTime.UtcNow
            };
        }
        catch
        {
            return new SearchRanking
            {
                Query = query,
                Results = Array.Empty<RankingResult>(),
                ComputedAt = DateTime.UtcNow
            };
        }
    }

    public Task<IReadOnlyList<TemplateRating>> GetTopRatedAsync(int count = 10, CancellationToken ct = default)
    {
        // Placeholder: V3 returns empty — V4+ will call the marketplace API
        return Task.FromResult<IReadOnlyList<TemplateRating>>(Array.Empty<TemplateRating>());
    }

    public Task<IReadOnlyList<TemplateRating>> GetTrendingAsync(int count = 10, CancellationToken ct = default)
    {
        // Placeholder: V3 returns empty — V4+ will call the marketplace API
        return Task.FromResult<IReadOnlyList<TemplateRating>>(Array.Empty<TemplateRating>());
    }
}
