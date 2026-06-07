namespace NextNet.Edge.Adapters;

/// <summary>
/// Edge runtime adapter for Deno Deploy.
/// Generates a <c>main.ts</c> entry point for Deno Deploy.
/// </summary>
public class DenoDeployAdapter : IEdgeRuntimeAdapter
{
    /// <inheritdoc />
    public string ProviderName => "Deno Deploy";

    /// <inheritdoc />
    public string ProviderId => "deno";

    /// <inheritdoc />
    public Task<IEdgeResponse> HandleRequestAsync(IEdgeRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        cancellationToken.ThrowIfCancellationRequested();

        var responseBody = new MemoryStream();
        var headers = new Dictionary<string, string>
        {
            ["x-edge-provider"] = ProviderId,
            ["x-edge-runtime"] = "deno-deploy"
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
    /// Generates a <c>main.ts</c> entry point for Deno Deploy.
    /// </summary>
    /// <param name="entryModule">The name of the compiled .NET entry module.</param>
    /// <returns>The main.ts content.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="entryModule"/> is null.</exception>
    public string GenerateDenoEntry(string entryModule)
    {
        if (entryModule == null) throw new ArgumentNullException(nameof(entryModule));

        return $$"""
import { {{entryModule}} } from './{{entryModule}}.js';

const entryPoint = new {{entryModule}}.NextNetDenoEntryPoint();

Deno.serve((request) => {
  return entryPoint.handle(request);
});
""";
    }
}
