using NextNet.Components;

namespace FixtureApp;

public class IndexPage : IPage
{
    public async Task<IHtmlContent> Render()
    {
        return new RawHtmlContent("<h1>Home</h1>");
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
}
