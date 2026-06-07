using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class RenderExceptionTests
{
    [Fact]
    public void Constructor_NoArguments_CreatesException()
    {
        var ex = new RenderException();
        Assert.NotNull(ex);
        Assert.Contains("RenderException", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new RenderException("Render failed.");
        Assert.Equal("Render failed.", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Inner issue");
        var ex = new RenderException("Outer error", inner);

        Assert.Equal("Outer error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_IsAssignableToException()
    {
        var ex = new RenderException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
