using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Abstractions.Rendering;

/// <summary>
/// Encapsulates contextual information required for rendering a UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="RenderContext"/> provides the renderer with access to the active
/// design tokens, application services, and the current theme name. This context
/// is passed through the rendering pipeline and enables consistent theming and
/// dependency injection across all component renderers.
/// </para>
/// <para>
/// Instances are typically created by the framework's rendering infrastructure
/// before invoking component renderers. The <see cref="Tokens"/> property provides
/// the complete set of design tokens for the active theme.
/// </para>
/// </remarks>
/// <param name="Tokens">The design token set for the current theme. Must not be null.</param>
/// <param name="Services">The application service provider for resolving dependencies.</param>
/// <param name="ThemeName">The name of the active theme, or <c>null</c> if using the default theme.</param>
public sealed record RenderContext(
    DesignTokenSet Tokens,
    IServiceProvider Services,
    string? ThemeName)
{
    /// <summary>
    /// Initializes a new instance of <see cref="RenderContext"/> with the specified
    /// token set and service provider, using the default theme.
    /// </summary>
    /// <param name="tokens">The design token set for the current theme.</param>
    /// <param name="services">The application service provider for resolving dependencies.</param>
    public RenderContext(DesignTokenSet tokens, IServiceProvider services)
        : this(tokens, services, null)
    {
    }
}
