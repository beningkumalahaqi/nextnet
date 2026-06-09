using NextNet.UI.Abstractions.Components;
using Xunit;

namespace NextNet.UI.Abstractions.Tests.Components;

public class ComponentVariantTests
{
    [Fact]
    public void Primary_Should_HaveNamePrimary()
    {
        var variant = ComponentVariant.Primary;
        Assert.Equal("Primary", variant.Name);
    }

    [Fact]
    public void Secondary_Should_HaveNameSecondary()
    {
        var variant = ComponentVariant.Secondary;
        Assert.Equal("Secondary", variant.Name);
    }

    [Fact]
    public void Danger_Should_HaveNameDanger()
    {
        var variant = ComponentVariant.Danger;
        Assert.Equal("Danger", variant.Name);
    }

    [Fact]
    public void Ghost_Should_HaveNameGhost()
    {
        var variant = ComponentVariant.Ghost;
        Assert.Equal("Ghost", variant.Name);
    }

    [Fact]
    public void Outline_Should_HaveNameOutline()
    {
        var variant = ComponentVariant.Outline;
        Assert.Equal("Outline", variant.Name);
    }

    [Fact]
    public void Success_Should_HaveNameSuccess()
    {
        var variant = ComponentVariant.Success;
        Assert.Equal("Success", variant.Name);
    }

    [Fact]
    public void Warning_Should_HaveNameWarning()
    {
        var variant = ComponentVariant.Warning;
        Assert.Equal("Warning", variant.Name);
    }

    [Fact]
    public void Info_Should_HaveNameInfo()
    {
        var variant = ComponentVariant.Info;
        Assert.Equal("Info", variant.Name);
    }

    [Fact]
    public void AllVariants_Should_BeSingleton()
    {
        Assert.Same(ComponentVariant.Primary, ComponentVariant.Primary);
        Assert.Same(ComponentVariant.Secondary, ComponentVariant.Secondary);
        Assert.Same(ComponentVariant.Danger, ComponentVariant.Danger);
        Assert.Same(ComponentVariant.Ghost, ComponentVariant.Ghost);
        Assert.Same(ComponentVariant.Outline, ComponentVariant.Outline);
        Assert.Same(ComponentVariant.Success, ComponentVariant.Success);
        Assert.Same(ComponentVariant.Warning, ComponentVariant.Warning);
        Assert.Same(ComponentVariant.Info, ComponentVariant.Info);
    }

    [Fact]
    public void ToString_Should_ReturnName()
    {
        Assert.Equal("Primary", ComponentVariant.Primary.ToString());
        Assert.Equal("Danger", ComponentVariant.Danger.ToString());
    }

    [Fact]
    public void Constructor_Should_Throw_When_NameIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ComponentVariant(null!));
    }

    [Fact]
    public void Constructor_Should_Throw_When_NameIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => new ComponentVariant(string.Empty));
    }
}
