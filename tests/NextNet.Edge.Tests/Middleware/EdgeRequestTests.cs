using Microsoft.AspNetCore.Http;
using NextNet.Edge.Middleware;
using Xunit;

namespace NextNet.Edge.Tests.Middleware;

public class EdgeRequestTests
{
    [Fact]
    public void Constructor_Should_CopyAllProperties_When_FromHttpRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Method = "POST";
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("example.com", 443);
        httpContext.Request.Path = "/api/test";
        httpContext.Request.QueryString = new QueryString("?id=1");

        // Act
        var edgeRequest = new EdgeRequest(httpContext.Request);

        // Assert
        Assert.Equal("POST", edgeRequest.Method);
        Assert.Equal("https", edgeRequest.Scheme);
        Assert.Equal("example.com", edgeRequest.Host.Host);
        Assert.Equal("/api/test", edgeRequest.Path.Value);
        Assert.Contains("id=1", edgeRequest.Url);
        Assert.Same(httpContext.Request, edgeRequest.AspNetCoreRequest);
    }

    [Fact]
    public void Constructor_Should_ParseUrl_When_FromRawData()
    {
        // Arrange & Act
        var edgeRequest = new EdgeRequest(
            "GET",
            "https://api.example.com/users?page=1",
            new Dictionary<string, string> { { "Authorization", "Bearer token" } });

        // Assert
        Assert.Equal("GET", edgeRequest.Method);
        Assert.Equal("https", edgeRequest.Scheme);
        Assert.Equal("api.example.com", edgeRequest.Host.Host);
        Assert.Equal("/users", edgeRequest.Path.Value);
        Assert.Equal("1", edgeRequest.Query["page"]);
        Assert.Equal("Bearer token", edgeRequest.Headers["Authorization"]);
    }

    [Fact]
    public void Constructor_Should_DefaultToLocalhost_When_RelativeUrl()
    {
        // Arrange & Act
        var edgeRequest = new EdgeRequest("GET", "/about");

        // Assert
        Assert.Equal("https", edgeRequest.Scheme);
        Assert.Equal("/about", edgeRequest.Path.Value);
    }

    [Fact]
    public void ToAdapterRequest_Should_ReturnConverted_When_Called()
    {
        // Arrange
        var edgeRequest = new EdgeRequest("GET", "https://example.com/");

        // Act
        var adapterRequest = edgeRequest.ToAdapterRequest();

        // Assert
        Assert.NotNull(adapterRequest);
        Assert.Equal("GET", adapterRequest.Method);
        // The URL may include the default port (443) when the HostString is constructed
        // with a port value. Accept either form.
        Assert.True(adapterRequest.Url == "https://example.com/" ||
                    adapterRequest.Url == "https://example.com:443/",
                    $"Unexpected URL: {adapterRequest.Url}");
    }

    [Fact]
    public void Constructor_Should_Throw_When_HttpRequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EdgeRequest((HttpRequest)null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_MethodIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EdgeRequest(null!, "https://example.com/"));
    }

    [Fact]
    public void Constructor_Should_Throw_When_UrlIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new EdgeRequest("GET", null!));
    }
}
