using NextNet.Components;

namespace FixtureApp;

public class RootLayout : ILayout
{
    public async Task<IHtmlContent> Render(IHtmlContent children)
    {
        return children;
    }
}
