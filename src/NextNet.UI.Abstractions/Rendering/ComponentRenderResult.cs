using Microsoft.AspNetCore.Html;

namespace NextNet.UI.Abstractions.Rendering;

/// <summary>
/// Represents the result of rendering a UI component.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ComponentRenderResult"/> encapsulates the output of a component
/// rendering operation, including the generated HTML content and any warnings
/// that were produced during rendering.
/// </para>
/// <para>
/// Rendering warnings are non-fatal issues such as missing required properties,
/// deprecated configurations, or fallback behavior that was triggered.
/// Consumers can inspect the <see cref="Warnings"/> collection to surface
/// these issues during development.
/// </para>
/// </remarks>
/// <param name="Html">The rendered HTML content for the component.</param>
/// <param name="Warnings">A read-only list of warning messages produced during rendering.
/// Returns an empty list when there are no warnings.</param>
public sealed record ComponentRenderResult(
    IHtmlContent Html,
    IReadOnlyList<string> Warnings)
{
    /// <summary>
    /// Initializes a new instance of <see cref="ComponentRenderResult"/> with
    /// the specified HTML content and an empty warnings collection.
    /// </summary>
    /// <param name="html">The rendered HTML content.</param>
    public ComponentRenderResult(IHtmlContent html)
        : this(html, Array.Empty<string>())
    {
    }
}
