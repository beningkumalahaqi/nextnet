using System.Text;
using System.Text.RegularExpressions;
using NextNet.Components;

namespace NextNet.UI.Theming.Css;

/// <summary>
/// Builds complete <c>&lt;style&gt;</c> blocks containing CSS custom property declarations
/// for a given <see cref="Theme"/> and <see cref="CssVariableScope"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeStyleBuilder"/> wraps the output of <see cref="CssCustomPropertyGenerator.Generate"/>
/// in the appropriate CSS selector based on the requested scope:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="CssVariableScope.Root"/> → <c>:root { ... }</c></description></item>
///   <item><description><see cref="CssVariableScope.Theme"/> → <c>[data-theme="{themeName}"] { ... }</c></description></item>
///   <item><description><see cref="CssVariableScope.Component"/> → <c>.theme-{themeName} { ... }</c></description></item>
/// </list>
/// <para>
/// The result is returned as an <see cref="IHtmlContent"/> that renders a
/// <c>&lt;style&gt;</c> tag with the generated CSS.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var theme = LightTheme.Create();
/// var styleContent = ThemeStyleBuilder.Build(theme, CssVariableScope.Root);
/// // Produces: &lt;style&gt;:root { --color-gray-50: #F9FAFB; ... }&lt;/style&gt;
/// </code>
/// </example>
public static class ThemeStyleBuilder
{
    /// <summary>
    /// Regex pattern that allows only safe characters in theme names
    /// to prevent XSS injection into CSS selectors.
    /// </summary>
    private static readonly Regex SafeThemeNamePattern = new(
        "^[a-zA-Z0-9_-]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    /// <summary>
    /// Builds a <c>&lt;style&gt;</c> block containing CSS custom property declarations
    /// for the specified <paramref name="theme"/> at the given <paramref name="scope"/>.
    /// </summary>
    /// <param name="theme">The <see cref="Theme"/> whose tokens will be serialized. Must not be null.</param>
    /// <param name="scope">The <see cref="CssVariableScope"/> determining the CSS selector wrapper.</param>
    /// <returns>An <see cref="IHtmlContent"/> representing the complete <c>&lt;style&gt;</c> element.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="scope"/> is an undefined enum value. (Error DS-206)</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="theme"/> has a name with invalid characters.</exception>
    public static IHtmlContent Build(Theme theme, CssVariableScope scope)
    {
        ArgumentNullException.ThrowIfNull(theme);

        // Sanitize: reject theme names with unsafe characters (XSS prevention)
        if (theme.Name == null || !SafeThemeNamePattern.IsMatch(theme.Name))
        {
            throw new ArgumentException(
                $"Theme name '{theme.Name}' contains invalid characters. " +
                "Only alphanumeric characters, underscores, and hyphens are allowed.",
                nameof(theme));
        }

        var selector = scope switch
        {
            CssVariableScope.Root => ":root",
            CssVariableScope.Theme => $"[data-theme=\"{theme.Name}\"]",
            CssVariableScope.Component => $".theme-{theme.Name}",
            _ => throw new ArgumentException(
                $"DS-206: Unsupported CSS variable scope '{scope}'.", nameof(scope))
        };

        var cssVars = CssCustomPropertyGenerator.Generate(theme.Tokens);
        var sb = new StringBuilder();

        sb.Append("<style>").AppendLine();
        sb.Append(selector).AppendLine(" {");
        sb.Append(cssVars);
        sb.Append('}').AppendLine();
        sb.Append("</style>").AppendLine();

        return new RawHtmlContent(sb.ToString());
    }
}
