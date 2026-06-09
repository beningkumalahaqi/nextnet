using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class CloudflareWorkersAdapterTests
{
    [Fact]
    public void ProviderName_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new CloudflareWorkersAdapter();
        Assert.Equal("Cloudflare Workers", adapter.ProviderName);
    }

    [Fact]
    public void ProviderId_Should_ReturnCorrectValue_When_Accessed()
    {
        var adapter = new CloudflareWorkersAdapter();
        Assert.Equal("cloudflare", adapter.ProviderId);
    }

    [Fact]
    public async Task HandleRequestAsync_Should_ReturnResponseWithCorrectHeaders_When_Called()
    {
        // Arrange
        var adapter = new CloudflareWorkersAdapter();
        var request = new MockEdgeRequest("GET", "https://example.com/");

        // Act
        var response = await adapter.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Contains("x-edge-provider", response.Headers.Keys);
        Assert.Equal("cloudflare", response.Headers["x-edge-provider"]);
    }

    [Fact]
    public async Task HandleRequestAsync_Should_Throw_When_RequestIsNull()
    {
        var adapter = new CloudflareWorkersAdapter();
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            adapter.HandleRequestAsync(null!));
    }

    [Fact]
    public async Task GetStaticAssetsAsync_Should_ReturnEmpty_When_Called()
    {
        // Arrange
        var adapter = new CloudflareWorkersAdapter();

        // Act
        var assets = await adapter.GetStaticAssetsAsync();

        // Assert
        Assert.NotNull(assets);
        Assert.Empty(assets);
    }

    [Fact]
    public void GenerateWranglerConfig_Should_IncludeProjectName_When_Generated()
    {
        // Arrange
        var adapter = new CloudflareWorkersAdapter();

        // Act
        var config = adapter.GenerateWranglerConfig("my-app");

        // Assert
        Assert.Contains("my-app", config);
        Assert.Contains("name =", config);
        Assert.Contains("compatibility_date", config);
    }

    [Fact]
    public void GenerateWranglerConfig_Should_IncludeEnvironment_When_Specified()
    {
        // Arrange
        var adapter = new CloudflareWorkersAdapter();

        // Act
        var config = adapter.GenerateWranglerConfig("my-app", "staging");

        // Assert
        Assert.Contains("my-app-staging", config);
        Assert.Contains("staging", config);
    }

    [Fact]
    public void GenerateWranglerConfig_Should_Throw_When_NameIsNull()
    {
        var adapter = new CloudflareWorkersAdapter();
        Assert.Throws<ArgumentNullException>(() =>
            adapter.GenerateWranglerConfig(null!));
    }

    [Fact]
    public void GenerateWorkerEntry_Should_IncludeModuleName_When_Generated()
    {
        // Arrange
        var adapter = new CloudflareWorkersAdapter();

        // Act
        var entry = adapter.GenerateWorkerEntry("NextNetApp");

        // Assert
        Assert.Contains("NextNetApp", entry);
        Assert.Contains("fetch(request, env, ctx)", entry);
    }

    [Fact]
    public void GenerateWorkerEntry_Should_Throw_When_ModuleIsNull()
    {
        var adapter = new CloudflareWorkersAdapter();
        Assert.Throws<ArgumentNullException>(() =>
            adapter.GenerateWorkerEntry(null!));
    }

    private class MockEdgeRequest : IEdgeRequest
    {
        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream? Body { get; }

        public MockEdgeRequest(string method, string url, IReadOnlyDictionary<string, string>? headers = null)
        {
            Method = method;
            Url = url;
            Headers = headers ?? new Dictionary<string, string>();
            Body = null;
        }
    }
}
