using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class NextNetConfigurationExceptionTests
{
    [Fact]
    public void Constructor_NoArguments_CreatesException()
    {
        var ex = new NextNetConfigurationException();
        Assert.NotNull(ex);
        Assert.Contains("NextNetConfigurationException", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessage_SetsMessage()
    {
        var ex = new NextNetConfigurationException("Config is invalid.");
        Assert.Equal("Config is invalid.", ex.Message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_SetsBoth()
    {
        var inner = new InvalidOperationException("Root cause");
        var ex = new NextNetConfigurationException("Wrapped error", inner);

        Assert.Equal("Wrapped error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_IsAssignableToException()
    {
        var ex = new NextNetConfigurationException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
