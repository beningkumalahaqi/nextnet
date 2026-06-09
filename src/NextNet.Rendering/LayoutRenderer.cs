using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Exceptions;
using NextNet.Logging;
using NextNet.Rendering.Errors;

namespace NextNet.Rendering;

/// <summary>
/// Composes the layout chain for a page, wrapping the page content inside
/// nested layouts from innermost to outermost.
/// </summary>
public sealed class LayoutRenderer
{
    private readonly IRouteComponentResolver _componentResolver;
    private readonly INextNetLogger? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LayoutRenderer"/>.
    /// </summary>
    /// <param name="componentResolver">Resolver for layout types.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="componentResolver"/> is null.</exception>
    public LayoutRenderer(IRouteComponentResolver componentResolver, INextNetLogger? logger = null)
    {
        _componentResolver = componentResolver ?? throw new ArgumentNullException(nameof(componentResolver));
        _logger = logger;
    }

    /// <summary>
    /// Renders the page content wrapped inside the layout chain.
    /// Layouts are applied from innermost to outermost, so the page content
    /// is first wrapped by the nearest layout, then by the root layout.
    /// </summary>
    /// <param name="pageContent">The rendered page content.</param>
    /// <param name="layoutChain">Ordered list of layout file paths from nearest to root.</param>
    /// <param name="serviceProvider">The DI service provider for resolving layout instances.</param>
    /// <returns>The final HTML content with all layouts applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any required parameter is null.</exception>
    /// <exception cref="RenderException">Thrown when a layout type cannot be resolved or instantiated.</exception>
    public async Task<IHtmlContent> RenderAsync(
        IHtmlContent pageContent,
        IReadOnlyList<string> layoutChain,
        IServiceProvider serviceProvider)
    {
        if (pageContent == null) throw new ArgumentNullException(nameof(pageContent));
        if (layoutChain == null) throw new ArgumentNullException(nameof(layoutChain));
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        if (layoutChain.Count == 0)
        {
            _logger?.Debug("No layout chain — returning bare page content");
            return pageContent;
        }

        // Apply layouts inside-out: the first item in the chain is the nearest layout,
        // the last item is the root layout.
        IHtmlContent current = pageContent;

        foreach (var layoutPath in layoutChain)
        {
            var layout = ResolveLayout(layoutPath, serviceProvider);
            _logger?.Debug("Wrapping content in layout {LayoutPath} ({LayoutType})", layoutPath, layout.GetType().Name);
            current = await layout.Render(current);
        }

        return current;
    }

    /// <summary>
    /// Resolves a layout component from the given file path using DI.
    /// </summary>
    private ILayout ResolveLayout(string layoutPath, IServiceProvider serviceProvider)
    {
        var layoutType = _componentResolver.GetLayoutType(layoutPath)
            ?? throw new RenderException($"[{Errors.RenderingErrorCodes.LayoutTypeNotResolved}] Cannot resolve layout type for: {layoutPath}");

        try
        {
            return (ILayout)serviceProvider.GetRequiredService(layoutType);
        }
        catch (InvalidOperationException ex)
        {
            throw new RenderException(
                $"[{Errors.RenderingErrorCodes.LayoutTypeNotRegistered}] Layout type '{layoutType.FullName}' (from '{layoutPath}') is not registered in DI. " +
                "Ensure the layout is registered via services.AddScoped<ILayout, ...>().", ex);
        }
    }
}
