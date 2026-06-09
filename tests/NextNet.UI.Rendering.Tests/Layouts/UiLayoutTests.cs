using System.Threading.Tasks;
using NextNet.Components;
using NextNet.UI.Rendering.Layouts;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Layouts;

/// <summary>
/// Tests for <see cref="UiLayout"/> rendering behavior.
/// </summary>
public class UiLayoutTests
{
    [Fact]
    public async Task Render_Should_WrapContentWithHtmlShell_When_GivenChildren()
    {
        // Arrange
        var layout = new UiLayout
        {
            Title = "App Layout",
            ThemeName = "light"
        };
        var children = new RawHtmlContent("<main><p>Hello World</p></main>");

        // Act
        var html = (await layout.Render(children)).ToHtml();

        // Assert
        Assert.Contains("<!DOCTYPE html>", html);
        Assert.Contains("<html lang=\"en\">", html);
        Assert.Contains("<head>", html);
        Assert.Contains("<title>App Layout</title>", html);
        Assert.Contains("<body>", html);
        Assert.Contains("<main><p>Hello World</p></main>", html);
        Assert.Contains("</html>", html);
    }

    [Fact]
    public async Task Render_Should_IncludeFooter_When_ShowFooterIsTrue()
    {
        // Arrange
        var layout = new UiLayout
        {
            Title = "With Footer",
            ThemeName = "light",
            ShowFooter = true
        };
        var children = new RawHtmlContent("<p>Content</p>");

        // Act
        var html = (await layout.Render(children)).ToHtml();

        // Assert
        Assert.Contains("<footer", html);
        Assert.Contains("</footer>", html);
        Assert.Contains("NextNet Application", html);
    }

    [Fact]
    public async Task Render_Should_NotIncludeFooter_When_ShowFooterIsFalse()
    {
        // Arrange
        var layout = new UiLayout
        {
            Title = "No Footer",
            ThemeName = "light",
            ShowFooter = false
        };
        var children = new RawHtmlContent("<p>Content</p>");

        // Act
        var html = (await layout.Render(children)).ToHtml();

        // Assert
        Assert.DoesNotContain("<footer", html);
    }

    [Fact]
    public async Task Render_Should_UseCustomFooterContent_When_Provided()
    {
        // Arrange
        var layout = new UiLayout
        {
            Title = "Custom Footer",
            ThemeName = "light",
            ShowFooter = true,
            FooterContent = "<p>Custom Footer</p>"
        };
        var children = new RawHtmlContent("<p>Content</p>");

        // Act
        var html = (await layout.Render(children)).ToHtml();

        // Assert
        Assert.Contains("Custom Footer", html);
    }

    [Fact]
    public async Task RenderShell_Should_ReturnOpeningHtml_When_Called()
    {
        // Arrange
        var layout = new UiLayout
        {
            Title = "Streaming",
            ThemeName = "dark"
        };

        // Act
        var shell = (await layout.RenderShell()).ToHtml();

        // Assert
        Assert.Contains("<!DOCTYPE html>", shell);
        Assert.Contains("<html", shell);
        Assert.Contains("<head>", shell);
        Assert.Contains("<body>", shell);
    }

    [Fact]
    public async Task RenderFooter_Should_ReturnEmpty_When_ShowFooterIsFalse()
    {
        // Arrange
        var layout = new UiLayout
        {
            ShowFooter = false
        };

        // Act
        var footer = (await layout.RenderFooter()).ToHtml();

        // Assert
        Assert.Equal("", footer);
    }
}
