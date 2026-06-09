using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class DenoDeployAdapterTests
{
    [Fact]
    public void ProviderName_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new DenoDeployAdapter();
        Assert.Equal("Deno Deploy", adapter.ProviderName);
    }

    [Fact]
    public void ProviderId_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new DenoDeployAdapter();
        Assert.Equal("deno", adapter.ProviderId);
    }

    [Fact]
    public async Task HandleRequestAsync_Should_ReturnResponse_When_Called()
    {
        // Arrange
        var adapter = new DenoDeployAdapter();
        var request = new SimpleEdgeRequest("GET", "https://example.com/");

        // Act
        var response = await adapter.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("deno", response.Headers["x-edge-provider"]);
    }

    [Fact]
    public void GenerateDenoEntry_Should_IncludeModuleName_When_Generated()
    {
        // Arrange
        var adapter = new DenoDeployAdapter();

        // Act
        var entry = adapter.GenerateDenoEntry("NextNetApp");

        // Assert
        Assert.Contains("NextNetApp", entry);
        Assert.Contains("Deno.serve", entry);
    }

    private class SimpleEdgeRequest : IEdgeRequest
    {
        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream? Body { get; }

        public SimpleEdgeRequest(string method, string url)
        {
            Method = method;
            Url = url;
            Headers = new Dictionary<string, string>();
            Body = null;
        }
    }
}
