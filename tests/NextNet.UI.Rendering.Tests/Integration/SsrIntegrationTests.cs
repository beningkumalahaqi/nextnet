using System.Threading.Tasks;
using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Rendering.Composition;
using NextNet.UI.Rendering.Head;
using NextNet.UI.Rendering.Layouts;
using NextNet.UI.Rendering.Pages;
using Xunit;

namespace NextNet.UI.Rendering.Tests.Integration;

/// <summary>
/// Integration tests for the full UI rendering pipeline:
/// request → theme → component → HTML.
/// </summary>
public class SsrIntegrationTests
{
    [Fact]
    public async Task FullPipeline_Should_ProduceCompleteHtml_When_AllComponentsPresent()
    {
        // Arrange — create a full page with layout, components, theme, and head
        var component = new TestButton { ClassName = "btn-submit" };
        var componentNode = new ComponentNode(component);
        var tree = new[] { componentNode };

        var page = new UiPage
        {
            Title = "Integration Test",
            ThemeName = "dark",
            ComponentTree = tree
        };

        var layout = new UiLayout
        {
            Title = "App Shell",
            ThemeName = "dark",
            ShowFooter = true
        };

        // Act — render page and wrap in layout
        var pageContent = await page.Render();
        var finalHtml = (await layout.Render(pageContent)).ToHtml();

        // Assert — verify full document structure
        Assert.Contains("<!DOCTYPE html>", finalHtml);
        Assert.Contains("<html", finalHtml);
        Assert.Contains("<head>", finalHtml);
        Assert.Contains("Integration Test", finalHtml);
        Assert.Contains("App Shell", finalHtml);

        // Theme CSS
        Assert.Contains("<style>", finalHtml);

        // Body content
        Assert.Contains("<body>", finalHtml);

        // Footer
        Assert.Contains("<footer", finalHtml);

        Assert.Contains("</html>", finalHtml);
    }

    [Fact]
    public async Task ThemeHeadInjector_Should_AddCssToHead_When_UsedWithPage()
    {
        // Arrange
        var page = new UiPage
        {
            Title = "Themed Page",
            ThemeName = "custom-theme"
        };

        // Act
        var html = (await page.Render()).ToHtml();

        // Assert
        var headStart = html.IndexOf("<head>", System.StringComparison.Ordinal);
        var headEnd = html.IndexOf("</head>", System.StringComparison.Ordinal);
        Assert.True(headStart >= 0);
        Assert.True(headEnd > headStart);

        var headContent = html.Substring(headStart, headEnd - headStart + "</head>".Length);
        Assert.Contains("<style>", headContent);
    }

    [Fact]
    public void HeadContent_Should_AccumulateMultipleElements_When_Chained()
    {
        // Arrange
        var head = new HeadContent()
            .AddMeta("description", "Test page")
            .AddMeta("og:title", "Test")
            .AddTitle("Test Page")
            .AddStyle("body { color: red; }")
            .AddScript("console.log('hello');");

        // Act
        var rendered = head.Render();

        // Assert
        Assert.Contains("<meta name=\"description\"", rendered);
        Assert.Contains("<meta name=\"og:title\"", rendered);
        Assert.Contains("<title>Test Page</title>", rendered);
        Assert.Contains("<style>", rendered);
        Assert.Contains("body { color: red; }", rendered);
        Assert.Contains("<script>", rendered);
        Assert.Contains("console.log('hello');", rendered);
    }

    /// <summary>
    /// A minimal test button component for integration tests.
    /// </summary>
    private sealed class TestButton : IComponent
    {
        public string? ClassName { get; init; }
        public string? Style { get; init; }
        public string? Id { get; init; }
        public IReadOnlyList<IComponent> Children { get; init; } = Array.Empty<IComponent>();
    }
}
