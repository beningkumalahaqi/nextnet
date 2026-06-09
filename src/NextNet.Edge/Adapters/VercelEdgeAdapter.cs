namespace NextNet.Edge.Adapters;

/// <summary>
/// Edge runtime adapter for Vercel Edge Functions.
/// Generates <c>vercel.json</c> configuration and edge function entry points.
/// </summary>
public sealed class VercelEdgeAdapter : IEdgeRuntimeAdapter
{
    /// <summary>
    /// The template for vercel.json edge function configuration.
    /// </summary>
    public const string VercelConfigTemplate = """
{
  "functions": {
    "api/edge/**": {
      "runtime": "@vercel/edge@1.0.0",
      "maxDuration": 30
    }
  },
  "routes": [
    {
      "src": "/(.*)",
      "dest": "/api/edge/$1"
    }
  ]
}
""";

    /// <inheritdoc />
    public string ProviderName => "Vercel Edge Functions";

    /// <inheritdoc />
    public string ProviderId => "vercel";

    /// <inheritdoc />
    public Task<IEdgeResponse> HandleRequestAsync(IEdgeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();

        var responseBody = new MemoryStream();
        var headers = new Dictionary<string, string>
        {
            ["x-edge-provider"] = ProviderId,
            ["x-edge-runtime"] = "vercel-edge"
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
        return Task.FromResult<IReadOnlyList<StaticAsset>>(Array.Empty<StaticAsset>());
    }

    /// <summary>
    /// Generates a <c>vercel.json</c> configuration string.
    /// </summary>
    /// <returns>The vercel.json content.</returns>
    public string GenerateVercelConfig()
    {
        return VercelConfigTemplate;
    }

    /// <summary>
    /// Generates an edge function entry point for Vercel.
    /// </summary>
    /// <param name="entryModule">The name of the compiled .NET entry module.</param>
    /// <returns>The edge function source code.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryModule"/> is null.</exception>
    public string GenerateEdgeFunctionEntry(string entryModule)
    {
        if (entryModule == null) throw new ArgumentNullException(nameof(entryModule));

        return $$"""
import type { EdgeFunction } from '@vercel/edge';
import { {{entryModule}} } from './{{entryModule}}';

export const config = {
  runtime: '@vercel/edge',
};

export default (async (request) => {
  const adapter = new {{entryModule}}.NextNetVercelEntryPoint();
  return await adapter.handle(request);
}) satisfies EdgeFunction;
""";
    }
}
