namespace NextNet.UI.Rendering.Head;

/// <summary>
/// Defines the contract for providing content to be injected into the
/// <c>&lt;head&gt;</c> section of a page.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IHeadContentProvider"/> allows composable head content injection
/// from multiple sources such as theme providers, SEO plugins, and page metadata.
/// Implementations return a <see cref="HeadContent"/> instance containing all
/// elements to be injected.
/// </para>
/// <para>
/// Typical implementations include:
/// </para>
/// <list type="bullet">
///   <item><description>Theme providers that inject CSS variables and theme styles</description></item>
///   <item><description>SEO plugins that add meta tags and Open Graph properties</description></item>
///   <item><description>Analytics providers that inject tracking scripts</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public class SeoHeadProvider : IHeadContentProvider
/// {
///     public HeadContent GetHeadContent()
///     {
///         return new HeadContent()
///             .AddMeta("description", "My awesome page")
///             .AddMeta("og:title", "My Page");
///     }
/// }
/// </code>
/// </example>
public interface IHeadContentProvider
{
    /// <summary>
    /// Returns the <see cref="HeadContent"/> to be injected into the page head.
    /// </summary>
    /// <returns>A <see cref="HeadContent"/> instance containing meta tags, links,
    /// styles, and scripts to inject.</returns>
    HeadContent GetHeadContent();
}
