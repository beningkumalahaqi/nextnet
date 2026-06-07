namespace NextNet.TemplateMarketplace;

/// <summary>
/// HTTP client for communicating with the NextNet Marketplace API.
/// All methods handle network failures gracefully by returning null or empty collections.
/// </summary>
public sealed class MarketplaceApiClient
{
    private readonly HttpClient _http;
    private readonly MarketplaceOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>Initializes a new instance of the <see cref="MarketplaceApiClient"/>.</summary>
    public MarketplaceApiClient(HttpClient http, MarketplaceOptions options)
    {
        _http = http;
        _options = options;
        _http.BaseAddress = new Uri(options.Url);
    }

    /// <summary>Gets a publisher's profile by ID.</summary>
    public async Task<PublisherProfile?> GetPublisherProfileAsync(string publisherId, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<PublisherProfile>(
                $"/api/publishers/{Uri.EscapeDataString(publisherId)}", JsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    /// <summary>Gets aggregate ratings for a template.</summary>
    public async Task<IReadOnlyList<TemplateRating>> GetRatingsAsync(string templateName, CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<TemplateRating>>(
                $"/api/templates/{Uri.EscapeDataString(templateName)}/ratings", JsonOptions, ct);
            return result ?? new List<TemplateRating>();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<TemplateRating>();
        }
    }

    /// <summary>Submits a rating (and optional comment) for a template.</summary>
    public async Task<bool> SubmitRatingAsync(string templateName, int stars, string? comment, CancellationToken ct = default)
    {
        try
        {
            var payload = new { stars, comment };
            var response = await _http.PostAsJsonAsync(
                $"/api/templates/{Uri.EscapeDataString(templateName)}/ratings", payload, JsonOptions, ct);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    /// <summary>Gets all available template categories.</summary>
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<Category>>("/api/categories", JsonOptions, ct);
            return result ?? new List<Category>();
        }
        catch (HttpRequestException)
        {
            return Array.Empty<Category>();
        }
    }

    /// <summary>Gets download statistics for a template.</summary>
    public async Task<MarketplaceStats?> GetStatsAsync(string templateName, CancellationToken ct = default)
    {
        try
        {
            return await _http.GetFromJsonAsync<MarketplaceStats>(
                $"/api/templates/{Uri.EscapeDataString(templateName)}/stats", JsonOptions, ct);
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}
