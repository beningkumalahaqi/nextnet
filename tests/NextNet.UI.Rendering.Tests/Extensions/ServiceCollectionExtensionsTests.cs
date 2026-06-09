using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Rendering.Composition;
using NextNet.UI.Rendering.Extensions;
using NextNet.UI.Rendering.Head;
using NextNet.UI.Rendering.Layouts;
using NextNet.UI.Rendering.Pages;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Extensions;

/// <summary>
/// Tests for DI registration via <see cref="ServiceCollectionExtensions"/>.
/// </summary>
public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNextNetUiRendering_Should_RegisterComponentTreeRenderer_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering();

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(ComponentTreeRenderer));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_RegisterThemeHeadInjector_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering();

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(ThemeHeadInjector));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_RegisterUiPage_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering();

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(UiPage));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_RegisterUiPageOfT_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering();

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(UiPage<>));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_RegisterUiLayout_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering();

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(UiLayout));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_RegisterOptions_When_Called()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNextNetUiRendering(options =>
        {
            options.DefaultTheme = "dark";
        });

        // Assert
        Assert.Contains(result, d => d.ServiceType == typeof(UiRenderingOptions));
    }

    [Fact]
    public void AddNextNetUiRendering_Should_Throw_When_ServicesIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            ((IServiceCollection)null!).AddNextNetUiRendering());
    }
}
