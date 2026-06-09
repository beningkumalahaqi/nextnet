using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Extensions;
using NextNet.UI.DesignSystem.Rendering;
using Xunit;

namespace NextNet.UI.DesignSystem.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetDesignSystem_Should_RegisterDesignSystemOptions()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DesignSystemOptions>();

        Assert.NotNull(options);
        Assert.Equal("light", options.DefaultThemeName);
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_RegisterComponentRendererRegistry()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem();

        var provider = services.BuildServiceProvider();
        var registry = provider.GetRequiredService<ComponentRendererRegistry>();

        Assert.NotNull(registry);
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_RegisterAllComponentRenderers()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem();

        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IButton>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<ICard>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IInput>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IBadge>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IAvatar>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IAlert>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IModal>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IDropdown>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<ITable>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<ITabs>>());
        Assert.NotNull(provider.GetRequiredService<IComponentRenderer<IToggle>>());
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_ApplyCustomOptions()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem(options =>
        {
            options.DefaultThemeName = "dark";
            options.AutoRegisterComponents = false;
        });

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DesignSystemOptions>();

        Assert.Equal("dark", options.DefaultThemeName);
        Assert.False(options.AutoRegisterComponents);
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_RegisterRenderersAsSingletons()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem();

        var provider = services.BuildServiceProvider();

        var first = provider.GetRequiredService<IComponentRenderer<IButton>>();
        var second = provider.GetRequiredService<IComponentRenderer<IButton>>();

        Assert.Same(first, second);
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_Throw_WhenServicesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetDesignSystem());
    }

    [Fact]
    public void AddNextNetDesignSystem_Should_BeChainable()
    {
        var services = new ServiceCollection();

        var result = services.AddNextNetDesignSystem();

        Assert.Same(services, result);
    }

    [Fact]
    public void DefaultComponentRenderers_Should_ProduceHtml()
    {
        var services = new ServiceCollection();
        services.AddNextNetDesignSystem();

        var provider = services.BuildServiceProvider();
        var renderer = provider.GetRequiredService<IComponentRenderer<IButton>>();
        var button = new NextNet.UI.DesignSystem.Components.Button { Label = "Chain Test" };
        var context = new UI.Abstractions.Rendering.RenderContext(
            new NextNet.DesignSystem.Tokens.DesignTokenSet(),
            provider);

        var result = renderer.Render(button, context);

        Assert.NotNull(result.Html);
        Assert.Contains("Chain Test", result.Html.ToString());
    }
}
