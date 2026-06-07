using NextNet.Build.StaticGeneration;
using Xunit;

namespace NextNet.Build.Tests.StaticGeneration;

public class SsgResultTests
{
    [Fact]
    public void SsgResult_Success_WhenNoErrors()
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
    public void SsgResult_Failure_WhenErrorsExist()
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
    public void SsgResult_Empty_IsEmpty()
    {
        var result = SsgResult.Empty;

        Assert.True(result.Success);
        Assert.Empty(result.GeneratedFiles);
        Assert.Empty(result.Errors);
        Assert.Equal(0, result.PageCount);
        Assert.Equal(0, result.TotalBytes);
    }

    [Fact]
    public void SsgError_StoresProperties()
    {
        var ex = new InvalidOperationException("test");
        var error = new SsgError("/route", "Error message", ex);

        Assert.Equal("/route", error.Route);
        Assert.Equal("Error message", error.Message);
        Assert.Same(ex, error.Exception);
    }

    [Fact]
    public void SsgError_AllowsNullException()
    {
        var error = new SsgError("/route", "Error message");

        Assert.Equal("/route", error.Route);
        Assert.Equal("Error message", error.Message);
        Assert.Null(error.Exception);
    }
}
