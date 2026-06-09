using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.Rendering.Composition;
using NextNet.UI.Rendering.Head;

namespace NextNet.UI.Rendering.Pages;

/// <summary>
/// A theme-aware page implementation that renders a component tree with
/// configurable title and theme.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="UiPage"/> implements <see cref="IPage"/> and provides a complete
/// page rendering pipeline that includes:
/// </para>
/// <list type="bullet">
///   <item><description>Component tree rendering via <see cref="ComponentTreeRenderer"/></description></item>
///   <item><description>Theme-aware styling via the provided <see cref="ThemeName"/></description></item>
///   <item><description>Head content injection (title, meta tags, theme CSS)</description></item>
/// </list>
/// <para>
/// Use <see cref="UiPage{TState}"/> for data-driven pages that carry typed state.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var page = new UiPage
/// {
///     Title = "Home",
///     ThemeName = "light",
///     ComponentTree = new[] { new ComponentNode(new Button { Label = "Click" }) }
/// };
/// </code>
/// </example>
public class UiPage : IPage
{
    /// <summary>
    /// Gets or sets the component tree to render on this page.
    /// </summary>
    public IReadOnlyList<ComponentNode>? ComponentTree { get; set; }

    /// <summary>
    /// Gets or sets the page title rendered in the <c>&lt;title&gt;</c> tag.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets or sets the theme name used for styling this page.
    /// If null, the default theme is used.
    /// </summary>
    public string? ThemeName { get; init; }

    /// <summary>
    /// Gets or sets the component tree renderer for rendering the component hierarchy.
    /// If not set, a default instance is created from the service provider.
    /// </summary>
    protected ComponentTreeRenderer? TreeRenderer { get; set; }

    /// <summary>
    /// Gets or sets the render context for the current rendering operation.
    /// </summary>
    protected RenderContext? RenderContext { get; set; }

    /// <summary>
    /// Gets or sets the service provider for resolving dependencies.
    /// </summary>
    protected IServiceProvider? Services { get; set; }

    /// <summary>
    /// Gets or sets the head content provider for injecting elements into the page head.
    /// </summary>
    protected IHeadContentProvider? HeadContentProvider { get; set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> Props { get; protected set; }
        = new Dictionary<string, object>();

    /// <summary>
    /// Renders the page as a complete HTML document containing the component tree,
    /// head content, and theme styles.
    /// </summary>
    /// <returns>A task representing the asynchronous render operation, with the HTML content.</returns>
    public virtual Task<IHtmlContent> Render()
    {
        var headContent = new HeadContent();
        var themeInjector = new ThemeHeadInjector();

        // Build the page head
        if (!string.IsNullOrEmpty(Title))
        {
            headContent.AddTitle(Title);
        }

        // Inject theme CSS into head
        if (!string.IsNullOrEmpty(ThemeName) && HeadContentProvider != null)
        {
            var providerHead = HeadContentProvider.GetHeadContent();
            foreach (var meta in providerHead.MetaTags)
                headContent.AddMeta(meta.name, meta.content);
            foreach (var link in providerHead.Links)
                headContent.AddLink(link);
            if (!string.IsNullOrEmpty(providerHead.Title))
                headContent.AddTitle(providerHead.Title);
        }

        // Render component tree
        IHtmlContent bodyContent;
        if (ComponentTree != null && ComponentTree.Count > 0 && TreeRenderer != null && RenderContext != null)
        {
            bodyContent = TreeRenderer.RenderTree(ComponentTree, RenderContext);
        }
        else
        {
            bodyContent = new RawHtmlContent("");
        }

        // Inject theme styles
        var themeStyle = themeInjector.Inject(ThemeName);
        if (themeStyle != null)
        {
            headContent.AddStyle(themeStyle.ToHtml());
        }

        // Assemble the full HTML document
        var headHtml = headContent.Render();

        var html = $"<!DOCTYPE html>\n<html lang=\"en\">\n<head>\n{headHtml}\n</head>\n<body>\n{bodyContent.ToHtml()}\n</body>\n</html>";

        return Task.FromResult<IHtmlContent>(new RawHtmlContent(html));
    }
}
