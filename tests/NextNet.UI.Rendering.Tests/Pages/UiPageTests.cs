using System.Threading.Tasks;
using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Rendering.Composition;
using NextNet.UI.Rendering.Pages;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Pages;

/// <summary>
/// Tests for <see cref="UiPage"/> rendering behavior.
/// </summary>
public class UiPageTests
{
    [Fact]
    public async Task Render_Should_ProduceHtmlDocument_When_GivenComponentTree()
    {
        // Arrange
        var component = new TestButton
        {
            ClassName = "btn-primary"
        };

        var tree = new[] { new ComponentNode(component) };

        var page = new UiPage
        {
            Title = "Test Page",
            ThemeName = "light",
            ComponentTree = tree
        };

        // Act
        var html = (await page.Render()).ToHtml();

        // Assert
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<title>Test Page</title>", html);
        Assert.Contains("</html>", html);
    }

    [Fact]
    public async Task Render_Should_IncludeThemeStyle_When_ThemeNameProvided()
    {
        // Arrange
        var page = new UiPage
        {
            Title = "Themed Page",
            ThemeName = "dark"
        };

        // Act
        var html = (await page.Render()).ToHtml();

        // Assert
        Assert.Contains("<style>", html);
        Assert.Contains("</style>", html);
    }

    [Fact]
    public async Task Render_Should_NotIncludeTitle_When_TitleIsNull()
    {
        // Arrange
        var page = new UiPage
        {
            ThemeName = "light"
        };

        // Act
        var html = (await page.Render()).ToHtml();

        // Assert
        Assert.DoesNotContain("<title>", html);
    }

    [Fact]
    public async Task Render_Should_ReturnValidHtml_When_NoComponentTree()
    {
        // Arrange
        var page = new UiPage
        {
            Title = "Empty Page",
            ThemeName = "light"
        };

        // Act
        var html = (await page.Render()).ToHtml();

        // Assert
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("</html>", html);
    }

    /// <summary>
    /// A minimal test button component for page tests.
    /// </summary>
    private sealed class TestButton : IComponent
    {
        public string? ClassName { get; init; }
        public string? Style { get; init; }
        public string? Id { get; init; }
        public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();
    }
}
