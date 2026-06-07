using NextNet.Components;

namespace NextNet.Rendering.Tests.Fixtures.SampleApp.app;

/// <summary>
/// Home page component for the sample app.
/// </summary>
public class HomePage : IPage
{
    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>
    {
        ["title"] = "Home"
    };

    public async Task<IHtmlContent> Render()
    {
        await Task.CompletedTask;

        return HtmlHelper.Fragment(
            HtmlHelper.Element("h1", content: HtmlHelper.Text("Welcome to NextNet")),
            HtmlHelper.Element("p", content: HtmlHelper.Text("This is the home page rendered with SSR.")),
            HtmlHelper.Element("p", content: HtmlHelper.Raw("Raw <b>HTML</b> content here."))
        );
    }
}
