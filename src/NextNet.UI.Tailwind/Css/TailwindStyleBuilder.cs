using System.Text;

namespace NextNet.UI.Tailwind.Css;

/// <summary>
/// Builds HTML <c>&lt;style&gt;</c> tags containing Tailwind CSS directives
/// (<c>@tailwind base</c>, <c>@tailwind components</c>, <c>@tailwind utilities</c>)
/// for use in page layouts.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TailwindStyleBuilder"/> produces the <c>&lt;style&gt;</c> element
/// that bootstraps Tailwind CSS in the rendered HTML output. This is typically
/// placed in the <c>&lt;head&gt;</c> section of the layout or page template.
/// </para>
/// <para>
/// By default, the builder outputs all three standard Tailwind directives
/// (base, components, utilities). Optional custom CSS can be appended after
/// the directives via the <c>customCss</c> parameter.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = new TailwindStyleBuilder();
/// var html = builder.BuildStyleTag();
/// // Produces:
/// // &lt;style&gt;
/// //   @tailwind base;
/// //   @tailwind components;
/// //   @tailwind utilities;
/// // &lt;/style&gt;
/// </code>
/// </example>
public sealed class TailwindStyleBuilder
{
    /// <summary>
    /// Builds a complete <c>&lt;style&gt;</c> HTML element string with Tailwind CSS directives.
    /// </summary>
    /// <param name="includeBase">Whether to include the <c>@tailwind base</c> directive. Defaults to <c>true</c>.</param>
    /// <param name="includeComponents">Whether to include the <c>@tailwind components</c> directive. Defaults to <c>true</c>.</param>
    /// <param name="includeUtilities">Whether to include the <c>@tailwind utilities</c> directive. Defaults to <c>true</c>.</param>
    /// <param name="customCss">Optional custom CSS content to append after the Tailwind directives.</param>
    /// <returns>A <c>&lt;style&gt;</c> tag string with the specified Tailwind directives and custom CSS.</returns>
    public string BuildStyleTag(
        bool includeBase = true,
        bool includeComponents = true,
        bool includeUtilities = true,
        string? customCss = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<style>");

        if (includeBase)
        {
            sb.AppendLine("  @tailwind base;");
        }

        if (includeComponents)
        {
            sb.AppendLine("  @tailwind components;");
        }

        if (includeUtilities)
        {
            sb.AppendLine("  @tailwind utilities;");
        }

        if (!string.IsNullOrWhiteSpace(customCss))
        {
            sb.AppendLine();
            sb.AppendLine(customCss);
        }

        sb.AppendLine("</style>");

        return sb.ToString();
    }
}
