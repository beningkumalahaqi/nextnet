using Microsoft.AspNetCore.Http;
using NextNet.Edge.Middleware;
using Xunit;

namespace NextNet.Edge.Tests.Middleware;

public class EdgeHttpContextTests
{
    [Fact]
    public void Constructor_Should_SetProperties_When_Called()
    {
        // Arrange
        var request = new EdgeRequest("GET", "https://example.com/");
        var response = new EdgeResponse();

        // Act
        var edgeContext = new EdgeHttpContext(request, response);

        // Assert
        Assert.Same(request, edgeContext.Request);
        Assert.Same(response, edgeContext.Response);
        Assert.NotNull(edgeContext.Items);
        Assert.Empty(edgeContext.Items);
        Assert.NotNull(edgeContext.TraceIdentifier);
        Assert.NotNull(edgeContext.Connection);
        Assert.Null(edgeContext.AspNetCoreHttpContext);
    }

    [Fact]
    public void Constructor_Should_SetAspNetCoreHttpContext_When_Provided()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var request = new EdgeRequest("GET", "https://example.com/");
        var response = new EdgeResponse();

        // Act
        var edgeContext = new EdgeHttpContext(request, response, httpContext);

        // Assert
        Assert.Same(httpContext, edgeContext.AspNetCoreHttpContext);
    }

    [Fact]
    public void ToAspNetCoreHttpContext_Should_ReturnContext_When_UnderlyingExists()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var edgeContext = new EdgeHttpContext(
            new EdgeRequest("GET", "/"),
            new EdgeResponse(),
            httpContext);

        // Act
        var result = edgeContext.ToAspNetCoreHttpContext();

        // Assert
        Assert.Same(httpContext, result);
    }

    [Fact]
    public void ToAspNetCoreHttpContext_Should_Throw_When_NoUnderlyingContext()
    {
        // Arrange
        var edgeContext = new EdgeHttpContext(
            new EdgeRequest("GET", "/"),
            new EdgeResponse());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            edgeContext.ToAspNetCoreHttpContext());
    }

    [Fact]
    public void Items_Should_StoreAndRetrieve_When_Accessed()
    {
        // Arrange
        var edgeContext = new EdgeHttpContext(
            new EdgeRequest("GET", "/"),
            new EdgeResponse());

        // Act
        edgeContext.Items["key"] = "value";

        // Assert
        Assert.Equal("value", edgeContext.Items["key"]);
    }

    [Fact]
    public void ConnectionInfo_Should_HaveSettableProperties_When_Accessed()
    {
        // Arrange
        var edgeContext = new EdgeHttpContext(
            new EdgeRequest("GET", "/"),
            new EdgeResponse());

        // Act
        edgeContext.Connection.Country = "US";
        edgeContext.Connection.City = "New York";
        edgeContext.Connection.RemoteIpAddress = "203.0.113.1";

        // Assert
        Assert.Equal("US", edgeContext.Connection.Country);
        Assert.Equal("New York", edgeContext.Connection.City);
        Assert.Equal("203.0.113.1", edgeContext.Connection.RemoteIpAddress);
    }

    [Fact]
    public void Constructor_Should_Throw_When_RequestIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeHttpContext(null!, new EdgeResponse()));
    }

    [Fact]
    public void Constructor_Should_Throw_When_ResponseIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new EdgeHttpContext(new EdgeRequest("GET", "/"), null!));
    }
}
