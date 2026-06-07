using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Logging;

namespace NextNet.Layouts;

/// <summary>
/// Composes the layout chain for a page, wrapping the page content inside
/// nested layouts from innermost to outermost.
/// Works with pre-resolved layout types (see <see cref="LayoutChainResolver"/>).
/// </summary>
public class LayoutRenderer
{
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutRenderer"/>.
    /// </summary>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public LayoutRenderer(INextNetLogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// Renders the page content wrapped inside the given layout types.
    /// Layouts are applied from innermost to outermost, so the first type in
    /// <paramref name="layoutTypes"/> wraps the page first, then subsequent types
    /// wrap the result of the previous layout.
    /// </summary>
    /// <param name="pageContent">The rendered page content.</param>
    /// <param name="layoutTypes">Ordered list of layout types from innermost to outermost.</param>
    /// <param name="serviceProvider">The DI service provider for resolving layout instances.</param>
    /// <returns>The final HTML content with all layouts applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is <c>null</c>.</exception>
    /// <exception cref="RenderException">Thrown when a layout type cannot be instantiated from DI.</exception>
    public async Task<IHtmlContent> RenderAsync(
        IHtmlContent pageContent,
        IReadOnlyList<Type> layoutTypes,
        IServiceProvider serviceProvider)
    {
        if (pageContent == null) throw new ArgumentNullException(nameof(pageContent));
        if (layoutTypes == null) throw new ArgumentNullException(nameof(layoutTypes));
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        if (layoutTypes.Count == 0)
        {
            _logger?.Debug("No layout types — returning bare page content");
            return pageContent;
        }

        // Apply layouts inside-out: layoutTypes[0] is the nearest layout,
        // layoutTypes[^1] is the root layout.
        IHtmlContent current = pageContent;

        for (int i = 0; i < layoutTypes.Count; i++)
        {
            var layoutType = layoutTypes[i];
            var layout = ResolveLayoutInstance(layoutType, serviceProvider);
            _logger?.Debug("Wrapping content in layout [{Index}] {LayoutType}", i, layoutType.Name);
            current = await layout.Render(current);
        }

        return current;
    }

    /// <summary>
    /// Resolves a layout instance from the DI container.
    /// </summary>
    private static ILayout ResolveLayoutInstance(Type layoutType, IServiceProvider serviceProvider)
    {
        try
        {
            return (ILayout)serviceProvider.GetRequiredService(layoutType);
        }
        catch (InvalidOperationException ex)
        {
            throw new RenderException(
                $"Layout type '{layoutType.FullName}' is not registered in the DI container. " +
                "Ensure the layout is registered via services.AddScoped<ILayout, ...>() or " +
                "services.AddTransient<ILayout, ...>().", ex);
        }
    }
}
