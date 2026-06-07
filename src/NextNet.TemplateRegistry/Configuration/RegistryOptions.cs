namespace NextNet.TemplateRegistry;

/// <summary>
/// Configuration options for the template registry HTTP client and cache.
/// </summary>
public sealed class RegistryOptions
{
    /// <summary>
    /// Gets or sets the base URL of the template registry API.
    /// </summary>
    public string Url { get; set; } = "https://registry.nextnet.dev";

    /// <summary>
    /// Gets or sets the directory on disk used to cache registry responses.
    /// </summary>
    public string CacheDirectory { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".nextnet", "cache", "registry");

    /// <summary>
    /// Gets or sets the duration for which cached entries remain valid.
    /// </summary>
    public TimeSpan CacheTtl { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Gets or sets the HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed HTTP requests.
    /// </summary>
    public int MaxRetries { get; set; } = 3;
}
