using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class RouteDiscoveryExceptionTests
{
    [Fact]
    public void Constructor_NoArguments_CreatesException()
    {
        var ex = new RouteDiscoveryException();
        Assert.NotNull(ex);
        Assert.Contains("RouteDiscoveryException", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new RouteDiscoveryException("Route scan failed.");
        Assert.Equal("Route scan failed.", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("File not found");
        var ex = new RouteDiscoveryException("Discovery error", inner);

        Assert.Equal("Discovery error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_IsAssignableToException()
    {
        var ex = new RouteDiscoveryException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
