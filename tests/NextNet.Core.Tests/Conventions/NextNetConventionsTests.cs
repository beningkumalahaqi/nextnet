using NextNet.Conventions;
using Xunit;

namespace NextNet.Core.Tests.Conventions;

public class NextNetConventionsTests
{
    [Fact]
    public void ConfigFileName_IsCorrect()
    {
        Assert.Equal("nextnet.config.json", NextNetConventions.ConfigFileName);
    }

    [Fact]
    public void AppDirectory_IsCorrect()
    {
        Assert.Equal("app", NextNetConventions.AppDirectory);
    }

    [Fact]
    public void OutputDirectory_IsCorrect()
    {
        Assert.Equal("dist", NextNetConventions.OutputDirectory);
    }

    [Fact]
    public void PublicDirectory_IsCorrect()
    {
        Assert.Equal("public", NextNetConventions.PublicDirectory);
    }

    [Fact]
    public void PageFileName_IsCorrect()
    {
        Assert.Equal("page.cs", NextNetConventions.PageFileName);
    }

    [Fact]
    public void LayoutFileName_IsCorrect()
    {
        Assert.Equal("layout.cs", NextNetConventions.LayoutFileName);
    }

    [Fact]
    public void RouteFileName_IsCorrect()
    {
        Assert.Equal("route.cs", NextNetConventions.RouteFileName);
    }

    [Fact]
    public void ErrorFileName_IsCorrect()
    {
        Assert.Equal("error.cs", NextNetConventions.ErrorFileName);
    }

    [Fact]
    public void LoadingFileName_IsCorrect()
    {
        Assert.Equal("loading.cs", NextNetConventions.LoadingFileName);
    }

    [Fact]
    public void ReservedFileNames_ContainsAllReservedNames()
    {
        Assert.Contains(NextNetConventions.PageFileName, NextNetConventions.ReservedFileNames);
        Assert.Contains(NextNetConventions.LayoutFileName, NextNetConventions.ReservedFileNames);
        Assert.Contains(NextNetConventions.RouteFileName, NextNetConventions.ReservedFileNames);
        Assert.Contains(NextNetConventions.ErrorFileName, NextNetConventions.ReservedFileNames);
        Assert.Contains(NextNetConventions.LoadingFileName, NextNetConventions.ReservedFileNames);
        Assert.Equal(5, NextNetConventions.ReservedFileNames.Length);
    }

    [Theory]
    [InlineData("page.cs", true)]
    [InlineData("PAGE.cs", true)]
    [InlineData("Page.cs", true)]
    [InlineData("notpage.cs", false)]
    [InlineData("page.txt", false)]
    public void IsPageFile_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, NextNetConventions.IsPageFile(fileName));
    }

    [Theory]
    [InlineData("layout.cs", true)]
    [InlineData("LAYOUT.cs", true)]
    [InlineData("Layout.cs", true)]
    [InlineData("notlayout.cs", false)]
    [InlineData("layout.txt", false)]
    public void IsLayoutFile_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, NextNetConventions.IsLayoutFile(fileName));
    }

    [Theory]
    [InlineData("route.cs", true)]
    [InlineData("ROUTE.cs", true)]
    [InlineData("Route.cs", true)]
    [InlineData("notroute.cs", false)]
    [InlineData("route.txt", false)]
    public void IsRouteFile_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, NextNetConventions.IsRouteFile(fileName));
    }

    [Theory]
    [InlineData("error.cs", true)]
    [InlineData("ERROR.cs", true)]
    [InlineData("Error.cs", true)]
    [InlineData("noterror.cs", false)]
    [InlineData("error.txt", false)]
    public void IsErrorFile_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, NextNetConventions.IsErrorFile(fileName));
    }

    [Theory]
    [InlineData("page.cs", true)]
    [InlineData("layout.cs", true)]
    [InlineData("route.cs", true)]
    [InlineData("error.cs", true)]
    [InlineData("loading.cs", true)]
    [InlineData("random.cs", false)]
    [InlineData("page.txt", false)]
    [InlineData("PAGE.cs", true)]
    [InlineData("Layout.cs", true)]
    public void IsReservedFile_ReturnsExpected(string fileName, bool expected)
    {
        Assert.Equal(expected, NextNetConventions.IsReservedFile(fileName));
    }
}
