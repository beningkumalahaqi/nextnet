using Microsoft.AspNetCore.Http;
using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class ComponentContextTests
{
    [Fact]
    public void Constructor_Should_StoreHttpContext_When_Provided()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Same(httpContext, context.HttpContext);
    }

    [Fact]
    public void Constructor_Should_ThrowArgumentNullException_When_HttpContextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ComponentContext(null!));
    }

    [Fact]
    public void Constructor_Should_ParseQueryString_When_QueryParamsProvided()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?name=value&page=1");

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Equal(2, context.QueryParams.Count);
        Assert.Equal("value", context.QueryParams["name"]);
        Assert.Equal("1", context.QueryParams["page"]);
    }

    [Fact]
    public void Constructor_Should_ReturnEmptyQueryParams_When_NoQueryString()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.NotNull(context.QueryParams);
        Assert.Empty(context.QueryParams);
    }

    [Fact]
    public void Constructor_Should_ParseRouteValues_When_RouteParamsProvided()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["slug"] = "hello-world";
        httpContext.Request.RouteValues["id"] = "42";

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Equal(2, context.RouteParams.Count);
        Assert.Equal("hello-world", context.RouteParams["slug"]);
        Assert.Equal("42", context.RouteParams["id"]);
    }

    [Fact]
    public void Constructor_Should_ReturnEmptyRouteParams_When_NoRouteValues()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.NotNull(context.RouteParams);
        Assert.Empty(context.RouteParams);
    }

    [Fact]
    public void Constructor_Should_SkipNonStringRouteValues_When_Encountered()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues["intVal"] = 123; // non-string

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Empty(context.RouteParams);
    }

    [Fact]
    public void QueryParams_Should_BeCaseInsensitive_When_Accessed()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = new QueryString("?Name=John");

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Equal("John", context.QueryParams["name"]);
        Assert.Equal("John", context.QueryParams["NAME"]);
        Assert.Equal("John", context.QueryParams["Name"]);
    }
}
