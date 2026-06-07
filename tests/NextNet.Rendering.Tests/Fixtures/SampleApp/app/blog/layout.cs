using NextNet.Components;

namespace NextNet.Rendering.Tests.Fixtures.SampleApp.app.blog;

/// <summary>
/// Nested layout for the blog section. Wraps content in a blog-specific shell.
/// This is a nested layout inside the root layout.
/// </summary>
public class BlogLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        await Task.CompletedTask;

        var sidebar = HtmlHelper.Element("aside",
            attributes: new Dictionary<string, string> { ["class"] = "sidebar" },
            content: HtmlHelper.Element("h3", content: HtmlHelper.Text("Blog Sidebar")));

        var article = HtmlHelper.Element("article",
            attributes: new Dictionary<string, string> { ["class"] = "blog-content" },
            content: children);

        return HtmlHelper.Element("div",
            attributes: new Dictionary<string, string> { ["class"] = "blog-layout" },
            content: HtmlHelper.Fragment(sidebar, article));
    }
}
