using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class VercelEdgeAdapterTests
{
    [Fact]
    public void ProviderName_ReturnsCorrect()
    {
        var adapter = new VercelEdgeAdapter();
        Assert.Equal("Vercel Edge Functions", adapter.ProviderName);
    }

    [Fact]
    public void ProviderId_ReturnsCorrect()
    {
        var adapter = new VercelEdgeAdapter();
        Assert.Equal("vercel", adapter.ProviderId);
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsResponse()
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
    public void GenerateVercelConfig_ReturnsValid()
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
    public void GenerateEdgeFunctionEntry_IncludesModule()
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
