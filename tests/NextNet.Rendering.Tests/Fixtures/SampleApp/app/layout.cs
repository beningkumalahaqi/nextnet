using NextNet.Components;

namespace NextNet.Rendering.Tests.Fixtures.SampleApp.app;

/// <summary>
/// Root layout component for the sample app.
/// Wraps content in an HTML document shell with header and footer.
/// Supports progressive streaming via <see cref="RenderShell"/> and <see cref="RenderFooter"/>.
/// </summary>
public class RootLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        await Task.CompletedTask;
        return HtmlHelper.Fragment(
            await RenderShell(),
            children,
            await RenderFooter()
        );
    }

    public async Task<IHtmlContent> RenderShell()
    {
        await Task.CompletedTask;

        var head = HtmlHelper.Element("head",
            content: HtmlHelper.Fragment(
                HtmlHelper.Element("meta", new Dictionary<string, string>
                {
                    ["charset"] = "utf-8"
                }),
                HtmlHelper.Element("title", content: HtmlHelper.Text("Sample App")),
                HtmlHelper.Element("meta", new Dictionary<string, string>
                {
                    ["name"] = "viewport",
                    ["content"] = "width=device-width, initial-scale=1"
                })
            ));

        var header = HtmlHelper.Element("header",
            content: HtmlHelper.Element("nav",
                content: HtmlHelper.Fragment(
                    HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/" },
                        content: HtmlHelper.Text("Home")),
                    new RawHtmlContent(" | "),
                    HtmlHelper.Element("a", new Dictionary<string, string> { ["href"] = "/about" },
                        content: HtmlHelper.Text("About"))
                )));

        // Return shell up to but not including children — <main> is opened via raw HTML
        var shellBody = HtmlHelper.Fragment(
            header,
            new RawHtmlContent("<main class=\"content\">")
        );

        return HtmlHelper.Fragment(
            new RawHtmlContent("<!DOCTYPE html>"),
            HtmlHelper.Element("html", new Dictionary<string, string> { ["lang"] = "en" },
                content: HtmlHelper.Fragment(
                    head,
                    HtmlHelper.Element("body", content: shellBody)
                ))
        );
    }

    public async Task<IHtmlContent> RenderFooter()
    {
        await Task.CompletedTask;

        var footer = HtmlHelper.Element("footer",
            content: HtmlHelper.Text("\u00a9 2026 Sample App"));

        return HtmlHelper.Fragment(
            new RawHtmlContent("</main>"),
            footer,
            new RawHtmlContent("</body></html>")
        );
    }
}
