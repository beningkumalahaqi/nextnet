using Microsoft.AspNetCore.Http;
using NextNet.Edge.Middleware;
using Xunit;

namespace NextNet.Edge.Tests.Middleware;

public class EdgeResponseTests
{
    [Fact]
    public void Constructor_FromHttpResponse_CopiesProperties()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Response.StatusCode = 404;
        httpContext.Response.Headers["X-Custom"] = "value";

        // Act
        var edgeResponse = new EdgeResponse(httpContext.Response);

        // Assert
        Assert.Equal(404, edgeResponse.StatusCode);
        Assert.Equal("value", edgeResponse.Headers["X-Custom"]);
        Assert.Same(httpContext.Response, edgeResponse.AspNetCoreResponse);
    }

    [Fact]
    public void Constructor_FromScratch_SetsDefaults()
    {
        // Act
        var edgeResponse = new EdgeResponse();

        // Assert
        Assert.Equal(200, edgeResponse.StatusCode);
        Assert.NotNull(edgeResponse.Headers);
        Assert.Empty(edgeResponse.Headers);
        Assert.NotNull(edgeResponse.Body);
    }

    [Fact]
    public void Constructor_WithParameters_SetsValues()
    {
        // Arrange
        var headers = new Dictionary<string, string> { { "Content-Type", "text/html" } };
        var body = new MemoryStream();

        // Act
        var edgeResponse = new EdgeResponse(201, headers, body);

        // Assert
        Assert.Equal(201, edgeResponse.StatusCode);
        Assert.Equal("text/html", edgeResponse.ContentType);
        Assert.Same(body, edgeResponse.Body);
    }

    [Fact]
    public void ContentType_GetterSetter_Works()
    {
        // Arrange
        var edgeResponse = new EdgeResponse();

        // Act
        edgeResponse.ContentType = "application/json";

        // Assert
        Assert.Equal("application/json", edgeResponse.ContentType);
        Assert.Equal("application/json", edgeResponse.Headers["Content-Type"]);
    }

    [Fact]
    public void ContentLength_GetterSetter_Works()
    {
        // Arrange
        var edgeResponse = new EdgeResponse();

        // Act
        edgeResponse.ContentLength = 1024;

        // Assert
        Assert.Equal(1024, edgeResponse.ContentLength);
        Assert.Equal("1024", edgeResponse.Headers["Content-Length"]);
    }

    [Fact]
    public void SetHeader_SetsValue()
    {
        // Arrange
        var edgeResponse = new EdgeResponse();

        // Act
        edgeResponse.SetHeader("X-Test", "value");

        // Assert
        Assert.Equal("value", edgeResponse.Headers["X-Test"]);
    }

    [Fact]
    public async Task WriteAsync_WritesToBody()
    {
        // Arrange
        var edgeResponse = new EdgeResponse();
        var content = "Hello, Edge!";

        // Act
        await edgeResponse.WriteAsync(content);

        // Assert
        edgeResponse.Body.Position = 0;
        var reader = new StreamReader(edgeResponse.Body);
        var written = await reader.ReadToEndAsync();
        Assert.Equal(content, written);
    }

    [Fact]
    public void ToAdapterResponse_ReturnsConverted()
    {
        // Arrange
        var edgeResponse = new EdgeResponse(200,
            new Dictionary<string, string> { { "X-Test", "val" } });

        // Act
        var adapterResponse = edgeResponse.ToAdapterResponse();

        // Assert
        Assert.NotNull(adapterResponse);
        Assert.Equal(200, adapterResponse.StatusCode);
        Assert.Equal("val", adapterResponse.Headers["X-Test"]);
    }

    [Fact]
    public void Constructor_HttpResponse_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new EdgeResponse((HttpResponse)null!));
    }
}
