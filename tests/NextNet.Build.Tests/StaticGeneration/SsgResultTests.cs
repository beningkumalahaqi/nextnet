using NextNet.Build.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class SsgResultTests
{
    [Fact]
    public void SsgResult_Should_BeSuccess_When_NoErrors()
    {
        var result = new SsgResult(
            new[] { "index.html" },
            Array.Empty<SsgError>(),
            TimeSpan.FromSeconds(1),
            100,
            1,
            0);

        Assert.True(result.Success);
        Assert.Single(result.GeneratedFiles);
        Assert.Equal(TimeSpan.FromSeconds(1), result.Duration);
        Assert.Equal(100, result.TotalBytes);
        Assert.Equal(1, result.PageCount);
    }

    [Fact]
    public void SsgResult_Should_BeFailure_When_ErrorsExist()
    {
        var result = new SsgResult(
            Array.Empty<string>(),
            new[] { new SsgError("/error", "Something went wrong") },
            TimeSpan.Zero,
            0,
            0,
            0);

        Assert.False(result.Success);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void SsgResult_Should_BeEmpty_When_EmptyInstance()
    {
        var result = SsgResult.Empty;

        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.PageCount);
        Assert.Equal(0, result.TotalBytes);
    }

    [Fact]
    public void SsgError_Should_StoreProperties_When_Created()
    {
        var ex = new InvalidOperationException("test");
        var error = new SsgError("/route", "Error message", ex);

        Assert.Equal("/route", error.Route);
        Assert.Equal("Error message", error.Message);
        Assert.Same(ex, error.Exception);
    }

    [Fact]
    public void SsgError_Should_AllowNullException_When_NotProvided()
    {
        var error = new SsgError("/route", "Error message");

        Assert.Equal("/route", error.Route);
        Assert.Equal("Error message", error.Message);
        Assert.Null(error.Exception);
    }
}
