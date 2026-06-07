namespace NextNet.TemplateMarketplace;

/// <summary>
/// Configuration options for the NextNet Template Marketplace.
/// Controls API endpoint, caching behavior, and data collection settings.
/// </summary>
public sealed class MarketplaceOptions
{
    /// <summary>Base URL of the marketplace API.</summary>
    public string Url { get; set; } = "https://marketplace.nextnet.dev";

    /// <summary>Directory used for local cache of marketplace data.</summary>
    public string CacheDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "cache", "marketplace");

    /// <summary>Time-to-live for cached marketplace responses.</summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Whether anonymous usage data collection is enabled.
    /// This is opt-in only and disabled by default.
    /// </summary>
    public bool EnableDataCollection { get; set; } = false;

    /// <summary>Interval at which collected data is flushed to local storage.</summary>
    public TimeSpan DataCollectionFlushInterval { get; set; } = TimeSpan.FromMinutes(5);
}
