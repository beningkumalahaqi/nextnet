using Xunit;

namespace NextNet.UI.Theming.Tests;

public class DarkModeTests
{
    [Fact]
    public void DarkMode_Should_HaveLightValue()
    {
        Assert.Equal(0, (int)DarkMode.Light);
    }

    [Fact]
    public void DarkMode_Should_HaveDarkValue()
    {
        Assert.Equal(1, (int)DarkMode.Dark);
    }

    [Fact]
    public void DarkMode_Should_HaveSystemValue()
    {
        Assert.Equal(2, (int)DarkMode.System);
    }

    [Fact]
    public void DarkMode_Should_BeValidEnum()
    {
        Assert.True(Enum.IsDefined(typeof(DarkMode), DarkMode.Light));
        Assert.True(Enum.IsDefined(typeof(DarkMode), DarkMode.Dark));
        Assert.True(Enum.IsDefined(typeof(DarkMode), DarkMode.System));
    }
}
