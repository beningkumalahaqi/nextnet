using NextNet.Components;

namespace NextNet.Rendering.Tests.Fixtures.SampleApp.app.about;

/// <summary>
/// About page component for the sample app.
/// </summary>
public class AboutPage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>
    {
        ["title"] = "About"
    };

    public async Task<IHtmlContent> Render()
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("About NextNet")),
            HtmlHelper.Element("p", content: HtmlHelper.Text("NextNet is a modern full-stack web framework for .NET.")),
            HtmlHelper.Element("ul", content: HtmlHelper.Fragment(
                HtmlHelper.Element("li", content: HtmlHelper.Text("Server-side rendering")),
                HtmlHelper.Element("li", content: HtmlHelper.Text("File-based routing")),
                HtmlHelper.Element("li", content: HtmlHelper.Text("Streaming HTML"))
            ))
        );
    }
}
