using NextNet.Edge.Adapters;
using Xunit;

namespace NextNet.Edge.Tests.Adapters;

public class AwsLambdaEdgeAdapterTests
{
    [Fact]
    public void ProviderName_ReturnsCorrect()
    {
        var adapter = new AwsLambdaEdgeAdapter();
        Assert.Equal("AWS Lambda@Edge", adapter.ProviderName);
    }

    [Fact]
    public void ProviderId_ReturnsCorrect()
    {
        var adapter = new AwsLambdaEdgeAdapter();
        Assert.Equal("aws", adapter.ProviderId);
    }

    [Fact]
    public async Task HandleRequestAsync_ReturnsResponse()
    {
        // Arrange
        var adapter = new AwsLambdaEdgeAdapter();
        var request = new SimpleRequest("GET", "https://example.com/");

        // Act
        var response = await adapter.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(200, response.StatusCode);
        Assert.Equal("aws", response.Headers["x-edge-provider"]);
    }

    [Fact]
    public void GenerateCloudFormationTemplate_IncludesAllParams()
    {
        // Arrange
        var adapter = new AwsLambdaEdgeAdapter();

        // Act
        var template = adapter.GenerateCloudFormationTemplate(
            "my-function",
            "MyApp::MyApp.Edge.Handler::Handler",
            "my-bucket",
            "my-app-edge.zip");

        // Assert
        Assert.Contains("my-function", template);
        Assert.Contains("MyApp::MyApp.Edge.Handler::Handler", template);
        Assert.Contains("my-bucket", template);
        Assert.Contains("my-app-edge.zip", template);
        Assert.Contains("AWS::Lambda::Function", template);
    }

    [Theory]
    [InlineData(null, "handler", "bucket", "key")]
    [InlineData("name", null, "bucket", "key")]
    [InlineData("name", "handler", null, "key")]
    [InlineData("name", "handler", "bucket", null)]
    public void GenerateCloudFormationTemplate_NullParam_Throws(
        string? name, string? handler, string? bucket, string? key)
    {
        var adapter = new AwsLambdaEdgeAdapter();
        Assert.Throws<ArgumentNullException>(() =>
            adapter.GenerateCloudFormationTemplate(name!, handler!, bucket!, key!));
    }

    private class SimpleRequest : IEdgeRequest
    {
        public string Method { get; }
        public string Url { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
        public Stream? Body { get; }

        public SimpleRequest(string method, string url)
        {
            Method = method;
            Url = url;
            Headers = new Dictionary<string, string>();
            Body = null;
        }
    }
}
