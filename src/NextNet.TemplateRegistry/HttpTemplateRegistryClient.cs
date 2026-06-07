using System.Net.Http.Json;
using System.Text.Json;
using NextNet.Templates.Abstractions;

namespace NextNet.TemplateRegistry;

/// <summary>
/// Low-level HTTP client for communicating with the NextNet template registry API.
/// </summary>
/// <remarks>
/// Handles HTTP calls, JSON deserialization, retry with exponential backoff, and
/// error mapping to domain exceptions. This class is consumed internally by
/// <see cref="TemplateRegistry"/> and is public only to support DI registration.
/// Applications should use <see cref="ITemplateRegistry"/> instead.
/// </remarks>
public sealed class HttpTemplateRegistryClient
{
    private readonly HttpClient _http;
    private readonly RegistryOptions _options;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpTemplateRegistryClient"/> class.
    /// </summary>
    /// <param name="http">The <see cref="HttpClient"/> managed by the HTTP client factory.</param>
    /// <param name="options">The registry configuration options.</param>
    public HttpTemplateRegistryClient(HttpClient http, RegistryOptions options)
    {
        _http = http;
        _options = options;
        _http.BaseAddress = new Uri(options.Url);
        _http.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
    }

    /// <summary>
    /// Searches for templates matching the given query.
    /// </summary>
    public async Task<TemplateSearchResult> SearchAsync(string query, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var url = $"/api/templates/search?q={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}";
        return await GetAsync<TemplateSearchResult>(url, ct);
    }

    /// <summary>
    /// Gets metadata for a specific template, or <c>null</c> if not found.
    /// </summary>
    public async Task<TemplateMetadata?> GetMetadataAsync(string name, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<TemplateMetadata>($"/api/templates/{Uri.EscapeDataString(name)}", ct);
        }
        catch (RegistryNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the list of all versions for a template.
    /// </summary>
    public async Task<IReadOnlyList<TemplateVersionInfo>> GetVersionsAsync(string name, CancellationToken ct = default)
    {
        return await GetAsync<List<TemplateVersionInfo>>($"/api/templates/{Uri.EscapeDataString(name)}/versions", ct);
    }

    /// <summary>
    /// Gets the latest version info for a template, or <c>null</c> if not found.
    /// </summary>
    public async Task<TemplateVersionInfo?> GetLatestVersionAsync(string name, CancellationToken ct = default)
    {
        try
        {
            return await GetAsync<TemplateVersionInfo>($"/api/templates/{Uri.EscapeDataString(name)}/latest", ct);
        }
        catch (RegistryNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Downloads a specific version of a template and returns its content stream.
    /// </summary>
    public async Task<TemplateDownloadInfo> DownloadAsync(string name, string version, CancellationToken ct = default)
    {
        var response = await ExecuteWithRetryAsync(
            () => _http.GetAsync($"/api/templates/{Uri.EscapeDataString(name)}/{Uri.EscapeDataString(version)}/download", ct),
            ct);

        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync(ct);
        var downloadInfo = new TemplateDownloadInfo
        {
            Name = name,
            Version = version,
            ChecksumSha256 = response.Headers.TryGetValues("X-Checksum-SHA256", out var values) ? values.First() : "",
            SizeBytes = response.Content.Headers.ContentLength ?? 0,
            Content = stream
        };
        return downloadInfo;
    }

    private async Task<T> GetAsync<T>(string url, CancellationToken ct)
    {
        var response = await ExecuteWithRetryAsync(() => _http.GetAsync(url, ct), ct);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            throw new RegistryNotFoundException($"Resource not found: {url}");

        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter?.Delta ?? TimeSpan.FromSeconds(60);
            throw new RateLimitException(retryAfter);
        }

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<T>(JsonOptions, ct);
        return result ?? throw new RegistryUnavailableException("Empty response from registry");
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(
        Func<Task<HttpResponseMessage>> action,
        CancellationToken ct)
    {
        Exception? lastException = null;
        for (int attempt = 0; attempt < _options.MaxRetries; attempt++)
        {
            try
            {
                var response = await action();
                if (response.IsSuccessStatusCode || ((int)response.StatusCode >= 400 && (int)response.StatusCode < 500))
                    return response;

                lastException = new RegistryUnavailableException($"HTTP {(int)response.StatusCode}");
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
            }

            if (attempt < _options.MaxRetries - 1)
            {
                var delay = TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt));
                await Task.Delay(delay, ct);
            }
        }
        throw new RegistryUnavailableException("All retries exhausted", lastException);
    }
}
