namespace NextNet.Edge.Adapters;

/// <summary>
/// Edge runtime adapter for Cloudflare Workers.
/// Generates <c>wrangler.toml</c> configuration and <c>worker.js</c> entry points.
/// </summary>
public sealed class CloudflareWorkersAdapter : IEdgeRuntimeAdapter
{
    /// <summary>
    /// Gets the provider-specific configuration prefix for wrangler.toml.
    /// </summary>
    public const string WranglerConfigTemplate = """
name = "{name}"
main = "worker.js"
compatibility_date = "{date}"

[site]
bucket = "./public"

[env.{env}]
name = "{name}-{env}"

""";

    /// <inheritdoc />
    public string ProviderName => "Cloudflare Workers";

    /// <inheritdoc />
    public string ProviderId => "cloudflare";

    /// <summary>
    /// The Cloudflare account ID (optional, configured via EdgeOptions).
    /// </summary>
    public string? AccountId { get; set; }

    /// <summary>
    /// The Cloudflare zone ID (optional, configured via EdgeOptions).
    /// </summary>
    public string? ZoneId { get; set; }

    /// <summary>
    /// The route pattern (e.g., "*.example.com/*").
    /// </summary>
    public string? Route { get; set; }

    /// <inheritdoc />
    public Task<IEdgeResponse> HandleRequestAsync(IEdgeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();

        // Cloudflare Workers entry point will be generated at build time.
        // This method is used for testing and preview mode.
        var responseBody = new MemoryStream();
        var headers = new Dictionary<string, string>
        {
            ["x-edge-provider"] = ProviderId,
            ["x-edge-runtime"] = "cloudflare-workers"
        };

        var edgeResponse = new EdgeResponse(
            statusCode: 200,
            headers: headers,
            body: responseBody);

        return Task.FromResult<IEdgeResponse>(edgeResponse);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<StaticAsset>> GetStaticAssetsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // At build time, the static asset pipeline populates this.
        // For now, return an empty list.
        return Task.FromResult<IReadOnlyList<StaticAsset>>(Array.Empty<StaticAsset>());
    }

    /// <summary>
    /// Generates a <c>wrangler.toml</c> configuration string for this adapter.
    /// </summary>
    /// <param name="projectName">The project/application name.</param>
    /// <param name="environment">The deployment environment (e.g., "production", "staging").</param>
    /// <returns>The wrangler.toml content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="projectName"/> is null.</exception>
    public string GenerateWranglerConfig(string projectName, string? environment = null)
    {
        if (projectName == null) throw new ArgumentNullException(nameof(projectName));

        environment ??= "production";
        var date = DateTime.UtcNow.ToString("yyyy-MM-dd");

        return WranglerConfigTemplate
            .Replace("{name}", projectName)
            .Replace("{date}", date)
            .Replace("{env}", environment);
    }

    /// <summary>
    /// Generates a <c>worker.js</c> entry point JavaScript string.
    /// </summary>
    /// <param name="entryModule">The name of the compiled .NET entry module.</param>
    /// <returns>The worker.js content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryModule"/> is null.</exception>
    public string GenerateWorkerEntry(string entryModule)
    {
        if (entryModule == null) throw new ArgumentNullException(nameof(entryModule));

        return $$"""
import { {{entryModule}} } from "./{{entryModule}}.js";

export default {
  async fetch(request, env, ctx) {
    const adapter = new {{entryModule}}.NextNetCloudflareEntryPoint();
    const response = await adapter.fetch(request);
    return response;
  },
};
""";
    }
}
