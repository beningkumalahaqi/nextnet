using NextNet.UI.Abstractions.Components;
using Xunit;

namespace NextNet.UI.Abstractions.Tests.Components;

public class ComponentSizeTests
{
    [Fact]
    public void Sm_Should_HaveNameSm()
    {
        var size = ComponentSize.Sm;
        Assert.Equal("Sm", size.Name);
    }

    [Fact]
    public void Md_Should_HaveNameMd()
    {
        var size = ComponentSize.Md;
        Assert.Equal("Md", size.Name);
    }

    [Fact]
    public void Lg_Should_HaveNameLg()
    {
        var size = ComponentSize.Lg;
        Assert.Equal("Lg", size.Name);
    }

    [Fact]
    public void Xl_Should_HaveNameXl()
    {
        var size = ComponentSize.Xl;
        Assert.Equal("Xl", size.Name);
    }

    [Fact]
    public void AllSizes_Should_BeSingleton()
    {
        Assert.Same(ComponentSize.Sm, ComponentSize.Sm);
        Assert.Same(ComponentSize.Md, ComponentSize.Md);
        Assert.Same(ComponentSize.Lg, ComponentSize.Lg);
        Assert.Same(ComponentSize.Xl, ComponentSize.Xl);
    }

    [Fact]
    public void ToString_Should_ReturnName()
    {
        Assert.Equal("Sm", ComponentSize.Sm.ToString());
        Assert.Equal("Lg", ComponentSize.Lg.ToString());
    }

    [Fact]
    public void Constructor_Should_Throw_When_NameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ComponentSize(null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_NameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ComponentSize(string.Empty));
    }
}
