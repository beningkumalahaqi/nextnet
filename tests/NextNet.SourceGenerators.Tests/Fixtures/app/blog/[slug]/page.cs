using NextNet.Components;

namespace FixtureApp;

public class BlogSlugPage : IPage
{
    public string Slug { get; set; } = string.Empty;

    public async Task<IHtmlContent> Render()
    {
        return new RawHtmlContent($"<h1>Blog: {Slug}</h1>");
    }

    public IReadOnlyDictionary<string, object> Props { get; } = new Dictionary<string, object>();
}
