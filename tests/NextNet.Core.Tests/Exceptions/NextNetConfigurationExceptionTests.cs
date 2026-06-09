using NextNet.Exceptions;
using Xunit;

namespace NextNet.Core.Tests.Exceptions;

public class NextNetConfigurationExceptionTests
{
    [Fact]
    public void Constructor_Should_CreateException_When_NoArguments()
    {
        var ex = new NextNetConfigurationException();
        Assert.NotNull(ex);
        Assert.Contains("NextNetConfigurationException", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessage_When_MessageProvided()
    {
        var ex = new NextNetConfigurationException("Config is invalid.");
        Assert.Equal("Config is invalid.", ex.Message);
    }

    [Fact]
    public void Constructor_Should_SetMessageAndInnerException_When_BothProvided()
    {
        var inner = new InvalidOperationException("Root cause");
        var ex = new NextNetConfigurationException("Wrapped error", inner);

        Assert.Equal("Wrapped error", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void Constructor_Should_BeAssignableToException_When_Invoked()
    {
        var ex = new NextNetConfigurationException("test");
        Assert.IsAssignableFrom<Exception>(ex);
    }
}
