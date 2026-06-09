using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class VercelEdgeAdapterTests
{
    [Fact]
    public void ProviderName_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new VercelEdgeAdapter();
        Assert.Equal("Vercel Edge Functions", adapter.ProviderName);
    }

    [Fact]
    public void ProviderId_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new VercelEdgeAdapter();
        Assert.Equal("vercel", adapter.ProviderId);
    }

    [Fact]
    public async Task HandleRequestAsync_Should_ReturnResponse_When_Called()
    {
        // Arrange
        var adapter = new VercelEdgeAdapter();
        var request = new TestEdgeRequest("GET", "https://example.com/");

        // Act
        var response = await adapter.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("vercel", response.Headers["x-edge-provider"]);
    }

    [Fact]
    public void GenerateVercelConfig_Should_ReturnValidConfig_When_Called()
    {
        // Arrange
        var adapter = new VercelEdgeAdapter();

        // Act
        var config = adapter.GenerateVercelConfig();

        // Assert
        Assert.Contains("vercel", config);
        Assert.Contains("runtime", config);
    }

    [Fact]
    public void GenerateEdgeFunctionEntry_Should_IncludeModuleName_When_Generated()
    {
        // Arrange
        var adapter = new VercelEdgeAdapter();

        // Act
        var entry = adapter.GenerateEdgeFunctionEntry("NextNetApp");

        // Assert
        Assert.Contains("NextNetApp", entry);
        Assert.Contains("@vercel/edge", entry);
    }

    private class TestEdgeRequest : IEdgeRequest
    {
        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream? Body { get; }

        public TestEdgeRequest(string method, string url)
        {
            Method = method;
            Url = url;
            Headers = new Dictionary<string, string>();
            Body = null;
        }
    }
}
