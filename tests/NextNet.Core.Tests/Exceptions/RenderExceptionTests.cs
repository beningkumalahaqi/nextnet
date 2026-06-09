using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class RenderExceptionTests
{
    [Fact]
    public void Constructor_Should_CreateException_When_NoArguments()
    {
        var ex = new RenderException();
        Assert.NotNull(ex);
        Assert.Contains("RenderException", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessage_When_MessageProvided()
    {
        var ex = new RenderException("Render failed.");
        Assert.Equal("Render failed.", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessageAndInnerException_When_BothProvided()
    {
        var inner = new InvalidOperationException("Inner issue");
        var ex = new RenderException("Outer error", inner);

        Assert.Equal("Outer error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_Should_BeAssignableToException_When_Invoked()
    {
        var ex = new RenderException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
