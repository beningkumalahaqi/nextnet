using NextNet.Components;

namespace FixtureApp;

public class AboutPage : IPage
{
    public async Task<IHtmlContent> Render()
    {
        return new RawHtmlContent("<h1>About</h1>");
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
}
