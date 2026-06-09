namespace NextNet.UI.Theming.Css;

/// <summary>
/// Defines the scope at which CSS custom properties (variables) are applied.
/// </summary>
/// <remarks>
/// <para>
/// The scope determines the CSS selector that wraps the generated custom properties:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Root"/> — variables are placed under <c>:root</c>, making them globally available.</description></item>
///   <item><description><see cref="Component"/> — variables are scoped to a specific component via a class or attribute selector.</description></item>
///   <item><description><see cref="Theme"/> — variables are scoped to a <c>[data-theme="{name}"]</c> attribute selector for per-them.</description></item>
/// </list>
/// </remarks>
public enum CssVariableScope
{
    /// <summary>
    /// Variables are declared under the <c>:root</c> pseudo-class, making them globally available
    /// across the entire document. Use for application-wide defaults.
    /// </summary>
    Root,

    /// <summary>
    /// Variables are scoped to a themed container via the <c>[data-theme="{themeName}"]</c> attribute
    /// selector. This enables multiple themes to coexist on the same page.
    /// </summary>
    Theme,

    /// <summary>
    /// Variables are scoped to a specific component. The generated CSS uses a class selector
    /// that can be applied to individual component root elements.
    /// </summary>
    Component,
}
