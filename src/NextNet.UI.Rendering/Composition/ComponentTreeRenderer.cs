using System.Text.Encodings.Web;
using NextNet.Components;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;

namespace NextNet.UI.Rendering.Composition;

/// <summary>
/// Recursively renders a <see cref="ComponentNode"/> tree using
/// <see cref="IComponentRenderer{T}"/> instances resolved from the
/// service provider.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentTreeRenderer"/> walks the component tree depth-first,
/// rendering each node via its registered <see cref="IComponentRenderer{T}"/>
/// and collecting the HTML output into a single <see cref="IHtmlContent"/>.
/// </para>
/// <para>
/// The renderer uses <see cref="RenderContext"/> (containing the active theme's
/// design tokens and services) during rendering. If a node's component type has
/// no registered renderer, a fallback HTML representation is produced.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var renderer = new ComponentTreeRenderer(serviceProvider);
/// var tree = new ComponentTreeBuilder()
///     .Add(new Button { Label = "Click me" })
///     .Build();
/// var html = renderer.RenderTree(tree, context);
/// </code>
/// </example>
public sealed class ComponentTreeRenderer
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="ComponentTreeRenderer"/>.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve <see cref="IComponentRenderer{T}"/> instances.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    public ComponentTreeRenderer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <summary>
    /// Renders the entire component tree rooted at the specified nodes.
    /// </summary>
    /// <param name="roots">The root-level component nodes to render.</param>
    /// <param name="context">The rendering context providing tokens and theme information.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered tree.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="roots"/> or <paramref name="context"/> is null.</exception>
    public IHtmlContent RenderTree(
        IReadOnlyList<ComponentNode> roots,
        RenderContext context)
    {
        if (roots == null) throw new ArgumentNullException(nameof(roots));
        if (context == null) throw new ArgumentNullException(nameof(context));

        var contents = new List<IHtmlContent>(roots.Count);
        foreach (var root in roots)
        {
            contents.Add(RenderNode(root, context));
        }

        return contents.Count == 1
            ? contents[0]
            : HtmlHelper.Fragment(contents.ToArray());
    }

    /// <summary>
    /// Renders a single <see cref="ComponentNode"/> and its children recursively.
    /// </summary>
    /// <param name="node">The component node to render.</param>
    /// <param name="context">The rendering context.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the rendered node.</returns>
    internal IHtmlContent RenderNode(ComponentNode node, RenderContext context)
    {
        // Try to find a registered renderer for the component type
        var componentType = node.Component.GetType();
        var rendererType = typeof(IComponentRenderer<>).MakeGenericType(componentType);
        var renderer = _serviceProvider.GetService(rendererType);

        IHtmlContent rendered;
        if (renderer != null)
        {
            // Use the component's own renderer
            var renderMethod = rendererType.GetMethod("Render");
            if (renderMethod != null)
            {
                var result = (ComponentRenderResult)renderMethod.Invoke(renderer, new object[] { node.Component, context })!;
                // Convert Microsoft.AspNetCore.Html.IHtmlContent to NextNet.Components.IHtmlContent
                using var sw = new StringWriter();
                result.Html.WriteTo(sw, HtmlEncoder.Default);
                rendered = new RawHtmlContent(sw.ToString());
            }
            else
            {
                rendered = FallbackRender(node.Component);
            }
        }
        else
        {
            rendered = FallbackRender(node.Component);
        }

        // Recursively render children
        if (node.Children.Count > 0)
        {
            var childContents = new List<IHtmlContent>(node.Children.Count);
            foreach (var child in node.Children)
            {
                childContents.Add(RenderNode(child, context));
            }
            var childrenHtml = HtmlHelper.Fragment(childContents.ToArray());

            // Wrap the parent content around children
            rendered = WrapParentAroundChildren(rendered, childrenHtml);
        }

        return rendered;
    }

    private static IHtmlContent FallbackRender(IComponent component)
    {
        var tagName = component.GetType().Name.ToLowerInvariant();
        var attrs = new Dictionary<string, string>();

        if (!string.IsNullOrEmpty(component.ClassName))
            attrs["class"] = component.ClassName;
        if (!string.IsNullOrEmpty(component.Id))
            attrs["id"] = component.Id;
        if (!string.IsNullOrEmpty(component.Style))
            attrs["style"] = component.Style;

        return HtmlHelper.Element("div", attrs, HtmlHelper.Text($"[{component.GetType().Name}]"));
    }

    private static IHtmlContent WrapParentAroundChildren(IHtmlContent parentContent, IHtmlContent childrenContent)
    {
        var parentHtml = parentContent.ToHtml();

        // Insert children before the closing tag of the parent element
        var closeTagIndex = parentHtml.LastIndexOf("</");
        if (closeTagIndex >= 0)
        {
            var beforeClose = parentHtml.Substring(0, closeTagIndex);
            var afterClose = parentHtml.Substring(closeTagIndex);
            var combined = beforeClose + childrenContent.ToHtml() + afterClose;
            return new RawHtmlContent(combined);
        }

        return new RawHtmlContent(parentHtml + childrenContent.ToHtml());
    }
}
