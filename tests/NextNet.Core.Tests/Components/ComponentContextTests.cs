using Microsoft.AspNetCore.Http;
using NextNet.Components;
using Xunit;

namespace NextNet.Core.Tests.Components;

public class ComponentContextTests
{
    [Fact]
    public void Constructor_WithHttpContext_StoresHttpContext()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        // Act
        var context = new ComponentContext(httpContext);

        // Assert
        Assert.Same(httpContext, context.HttpContext);
    }

    [Fact]
    public void Constructor_WithNullHttpContext_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ComponentContext(null!));
    }

    [Fact]
    public void Constructor_WithQueryParams_ParsesQueryString()
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
    public void Constructor_WithNoQueryParams_ReturnsEmptyQueryParams()
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
    public void Constructor_WithRouteParams_ParsesRouteValues()
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
    public void Constructor_WithNoRouteParams_ReturnsEmptyRouteParams()
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
    public void Constructor_WithNonStringRouteValue_SkipsIt()
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
    public void Constructor_QueryParamsIsCaseInsensitive()
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
