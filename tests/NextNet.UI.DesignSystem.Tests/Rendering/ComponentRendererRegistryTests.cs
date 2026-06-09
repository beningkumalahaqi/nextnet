using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.DesignSystem.Rendering;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Rendering;

public class ComponentRendererRegistryTests
{
    [Fact]
    public void Register_Should_StoreRendererByType()
    {
        var registry = new ComponentRendererRegistry();
        var renderer = new DefaultComponentRenderer<IButton>();

        registry.Register<IButton>(renderer);

        Assert.True(registry.IsRegistered<IButton>());
        Assert.Equal(1, registry.Count);
    }

    [Fact]
    public void Resolve_Should_ReturnRenderer_WhenRegistered()
    {
        var registry = new ComponentRendererRegistry();
        var renderer = new DefaultComponentRenderer<IButton>();
        registry.Register<IButton>(renderer);

        var resolved = registry.Resolve<IButton>();

        Assert.Same(renderer, resolved);
    }

    [Fact]
    public void Resolve_ByType_Should_ReturnRenderer_WhenRegistered()
    {
        var registry = new ComponentRendererRegistry();
        var renderer = new DefaultComponentRenderer<IButton>();
        registry.Register<IButton>(renderer);

        var resolved = registry.Resolve(typeof(IButton));

        Assert.Same(renderer, resolved);
    }

    [Fact]
    public void Resolve_Should_Throw_WhenNotRegistered()
    {
        var registry = new ComponentRendererRegistry();

        var ex = Assert.Throws<KeyNotFoundException>(() => registry.Resolve<IButton>());
        Assert.Contains("DS-152", ex.Message);
    }

    [Fact]
    public void Resolve_ByType_Should_Throw_WhenNotRegistered()
    {
        var registry = new ComponentRendererRegistry();

        var ex = Assert.Throws<KeyNotFoundException>(() => registry.Resolve(typeof(IButton)));
        Assert.Contains("DS-152", ex.Message);
    }

    [Fact]
    public void Register_Should_Throw_WhenDuplicate()
    {
        var registry = new ComponentRendererRegistry();
        registry.Register<IButton>(new DefaultComponentRenderer<IButton>());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            registry.Register<IButton>(new DefaultComponentRenderer<IButton>()));
        Assert.Contains("DS-151", ex.Message);
    }

    [Fact]
    public void RegisterOrReplace_Should_Overwrite_ExistingRegistration()
    {
        var registry = new ComponentRendererRegistry();
        var first = new DefaultComponentRenderer<IButton>();
        var second = new DefaultComponentRenderer<IButton>();
        registry.Register<IButton>(first);
        registry.RegisterOrReplace<IButton>(second);

        var resolved = registry.Resolve<IButton>();
        Assert.Same(second, resolved);
    }

    [Fact]
    public void IsRegistered_Should_ReturnFalse_WhenNotRegistered()
    {
        var registry = new ComponentRendererRegistry();

        Assert.False(registry.IsRegistered<IButton>());
    }

    [Fact]
    public void Count_Should_ReflectRegistrationCount()
    {
        var registry = new ComponentRendererRegistry();
        registry.Register<IButton>(new DefaultComponentRenderer<IButton>());
        registry.Register<ICard>(new DefaultComponentRenderer<ICard>());
        registry.Register<IInput>(new DefaultComponentRenderer<IInput>());

        Assert.Equal(3, registry.Count);
    }

    [Fact]
    public void Clear_Should_RemoveAllRegistrations()
    {
        var registry = new ComponentRendererRegistry();
        registry.Register<IButton>(new DefaultComponentRenderer<IButton>());
        registry.Register<ICard>(new DefaultComponentRenderer<ICard>());
        registry.Clear();

        Assert.Equal(0, registry.Count);
        Assert.False(registry.IsRegistered<IButton>());
    }

    [Fact]
    public void Register_Should_Throw_WhenRendererIsNull()
    {
        var registry = new ComponentRendererRegistry();
        Assert.Throws<ArgumentNullException>(() => registry.Register<IButton>(null!));
    }

    [Fact]
    public void Resolve_Generic_Should_Throw_WhenTypeIsNull()
    {
        var registry = new ComponentRendererRegistry();
        Assert.Throws<KeyNotFoundException>(() => registry.Resolve<IButton>());
    }

    [Fact]
    public void DefaultComponentRenderer_Should_RenderButtonCorrectly()
    {
        var registry = new ComponentRendererRegistry();
        registry.Register<IButton>(new DefaultComponentRenderer<IButton>());

        var button = new Button { Label = "Test", Variant = ComponentVariant.Primary };
        var context = new RenderContext(new DesignTokenSet(), new EmptyServiceProvider());

        var result = registry.Resolve<IButton>().Render(button, context);

        Assert.NotNull(result);
        Assert.NotNull(result.Html);
        var html = result.Html.ToString();
        Assert.Contains("btn-primary", html);
        Assert.Contains("Test", html);
    }
}
