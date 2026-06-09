using NextNet.DesignSystem.Tokens;
using Xunit;

namespace NextNet.DesignSystem.Tests.Tokens;

public class ColorTokenTests
{
    [Fact]
    public void Constructor_Should_SetNameAndValue()
    {
        var token = new ColorToken("primary-500", "#3B82F6");
        Assert.Equal("primary-500", token.Name);
        Assert.Equal("#3B82F6", token.Value);
    }

    [Fact]
    public void Constructor_Should_HaveNullOptionalProperties_When_NotSet()
    {
        var token = new ColorToken("primary-500", "#3B82F6");
        Assert.Null(token.Hover);
        Assert.Null(token.Active);
        Assert.Null(token.Foreground);
    }

    [Fact]
    public void WithInit_Should_SetHover()
    {
        var token = new ColorToken("primary-500", "#3B82F6") { Hover = "#2563EB" };
        Assert.Equal("#2563EB", token.Hover);
    }

    [Fact]
    public void WithInit_Should_SetActive()
    {
        var token = new ColorToken("primary-500", "#3B82F6") { Active = "#1D4ED8" };
        Assert.Equal("#1D4ED8", token.Active);
    }

    [Fact]
    public void WithInit_Should_SetForeground()
    {
        var token = new ColorToken("primary-500", "#3B82F6") { Foreground = "#FFFFFF" };
        Assert.Equal("#FFFFFF", token.Foreground);
    }

    [Fact]
    public void WithInit_Should_SetAllOptionalProperties()
    {
        var token = new ColorToken("primary-500", "#3B82F6")
        {
            Hover = "#2563EB",
            Active = "#1D4ED8",
            Foreground = "#FFFFFF"
        };
        Assert.Equal("#2563EB", token.Hover);
        Assert.Equal("#1D4ED8", token.Active);
        Assert.Equal("#FFFFFF", token.Foreground);
    }

    [Fact]
    public void Equality_Should_BeValueBased()
    {
        var token1 = new ColorToken("primary-500", "#3B82F6")
        {
            Hover = "#2563EB",
            Foreground = "#FFFFFF"
        };
        var token2 = new ColorToken("primary-500", "#3B82F6")
        {
            Hover = "#2563EB",
            Foreground = "#FFFFFF"
        };
        Assert.Equal(token1, token2);
        Assert.True(token1 == token2);
        Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
    }

    [Fact]
    public void Equality_Should_Differ_When_NameDiffers()
    {
        var token1 = new ColorToken("primary-500", "#3B82F6");
        var token2 = new ColorToken("primary-600", "#3B82F6");
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void Equality_Should_Differ_When_ValueDiffers()
    {
        var token1 = new ColorToken("primary-500", "#3B82F6");
        var token2 = new ColorToken("primary-500", "#2563EB");
        Assert.NotEqual(token1, token2);
    }

    [Fact]
    public void Record_Should_BeImmutable()
    {
        var token = new ColorToken("primary-500", "#3B82F6");
        var modified = token with { Hover = "#2563EB" };
        Assert.Null(token.Hover);
        Assert.Equal("#2563EB", modified.Hover);
    }

    [Fact]
    public void Deconstruct_Should_UnpackNameAndValue()
    {
        var (name, value) = new ColorToken("primary-500", "#3B82F6");
        Assert.Equal("primary-500", name);
        Assert.Equal("#3B82F6", value);
    }

    [Fact]
    public void ToString_Should_ContainName()
    {
        var token = new ColorToken("primary-500", "#3B82F6");
        Assert.Contains("primary-500", token.ToString());
        Assert.Contains("#3B82F6", token.ToString());
    }
}
