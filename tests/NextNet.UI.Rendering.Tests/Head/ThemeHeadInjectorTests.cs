using NextNet.UI.Rendering.Head;
using NextNet.UI.Theming.Css;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Head;

/// <summary>
/// Tests for <see cref="ThemeHeadInjector"/> CSS injection behavior.
/// </summary>
public class ThemeHeadInjectorTests
{
    [Fact]
    public void Inject_Should_ReturnStyleBlock_When_ThemeNameProvided()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act
        var result = injector.Inject("light");

        // Assert
        Assert.NotNull(result);
        var html = result.ToHtml();
        Assert.Contains("<style>", html);
        Assert.Contains("</style>", html);
    }

    [Fact]
    public void Inject_Should_ReturnDefaultStyle_When_ThemeNameIsNull()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act
        var result = injector.Inject(null);

        // Assert
        Assert.NotNull(result);
        var html = result.ToHtml();
        Assert.Contains("<style>", html);
        Assert.Contains("--color-primary-500", html);
    }

    [Fact]
    public void Inject_Should_ReturnDefaultStyle_When_ThemeNameIsEmpty()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act
        var result = injector.Inject(string.Empty);

        // Assert
        Assert.NotNull(result);
        var html = result.ToHtml();
        Assert.Contains("<style>", html);
    }

    [Fact]
    public void Inject_Should_UseThemeSelector_When_ThemeScopePassed()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act
        var result = injector.Inject("dark", CssVariableScope.Theme);

        // Assert
        Assert.NotNull(result);
        var html = result.ToHtml();
        Assert.Contains("data-theme=\"dark\"", html);
    }

    [Fact]
    public void Inject_Should_UseComponentClass_When_ComponentScopePassed()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act
        var result = injector.Inject("custom", CssVariableScope.Component);

        // Assert
        Assert.NotNull(result);
        var html = result.ToHtml();
        Assert.Contains("theme-custom", html);
    }

    [Fact]
    public void Inject_Should_Throw_When_ScopeIsUndefined()
    {
        // Arrange
        var injector = new ThemeHeadInjector();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => injector.Inject("light", (CssVariableScope)999));
    }
}
