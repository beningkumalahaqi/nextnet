using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.TemplateRegistry;

/// <summary>
/// The concrete template registry that discovers and downloads community templates
/// from the NextNet template registry API. Implements <see cref="ITemplateRegistry"/>.
/// </summary>
/// <remarks>
/// <para>
/// This registry wraps an <see cref="HttpTemplateRegistryClient"/> for HTTP communication
/// and a <see cref="TemplateRegistryCache"/> for local caching of search results and metadata.
/// </para>
/// <para>
/// When the registry is unavailable, search operations gracefully degrade to empty results
/// rather than throwing, enabling offline-first usage.
/// </para>
/// </remarks>
public sealed class TemplateRegistry : ITemplateRegistry
{
    private readonly HttpTemplateRegistryClient _client;
    private readonly TemplateRegistryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateRegistry"/> class.
    /// </summary>
    /// <param name="client">The HTTP client for registry API calls.</param>
    /// <param name="cache">The file-system cache for registry responses.</param>
    public TemplateRegistry(HttpTemplateRegistryClient client, TemplateRegistryCache cache)
    {
        _client = client;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<TemplateManifest>> SearchAsync(string query, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"search_{query}";
        var cached = await _cache.GetAsync<TemplateSearchResult>(cacheKey, cancellationToken);
        TemplateSearchResult? result;

        if (cached is not null)
        {
            result = cached;
        }
        else
        {
            try
            {
                result = await _client.SearchAsync(query, ct: cancellationToken);
                await _cache.SetAsync(cacheKey, result, cancellationToken);
            }
            catch (RegistryUnavailableException)
            {
                // Offline: return empty list
                return Array.Empty<TemplateManifest>();
            }
        }

        // Convert to TemplateManifest stubs (registry metadata only)
        return result.Items.Select(item => new TemplateManifest(
            Name: item.Name,
            Version: item.LatestVersion,
            NextNetVersion: ">=3.0.0",
            Author: item.Author,
            Description: item.Description,
            Tags: item.Tags
        )).ToList();
    }

    /// <inheritdoc />
    public async Task<TemplateManifest> GetManifestAsync(string templateName, string? version = null, CancellationToken cancellationToken = default)
    {
        var metadata = await _client.GetMetadataAsync(templateName, cancellationToken);
        if (metadata is null)
            throw new TemplateNotFoundException(templateName, version);

        return new TemplateManifest(
            Name: metadata.Name,
            Version: version ?? metadata.LatestVersion,
            NextNetVersion: ">=3.0.0",
            Author: metadata.Author,
            Description: metadata.Description,
            Tags: metadata.Tags
        );
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<string>> GetVersionsAsync(string templateName, CancellationToken cancellationToken = default)
    {
        var versions = await _client.GetVersionsAsync(templateName, cancellationToken);
        return versions.Select(v => v.Version).ToList();
    }

    /// <summary>
    /// Downloads a specific version of a template package from the registry.
    /// </summary>
    /// <param name="name">The template name.</param>
    /// <param name="version">The semantic version to download.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="TemplateDownloadInfo"/> containing the content stream and metadata.</returns>
    public async Task<TemplateDownloadInfo> DownloadAsync(string name, string version, CancellationToken cancellationToken = default)
    {
        return await _client.DownloadAsync(name, version, cancellationToken);
    }
}
