using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class RouteDiscoveryExceptionTests
{
    [Fact]
    public void Constructor_Should_CreateException_When_NoArguments()
    {
        var ex = new RouteDiscoveryException();
        Assert.NotNull(ex);
        Assert.Contains("RouteDiscoveryException", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessage_When_MessageProvided()
    {
        var ex = new RouteDiscoveryException("Route scan failed.");
        Assert.Equal("Route scan failed.", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessageAndInnerException_When_BothProvided()
    {
        var inner = new InvalidOperationException("File not found");
        var ex = new RouteDiscoveryException("Discovery error", inner);

        Assert.Equal("Discovery error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_Should_BeAssignableToException_When_Invoked()
    {
        var ex = new RouteDiscoveryException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
