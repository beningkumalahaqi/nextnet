using System.Text;
using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Theming.Css;

/// <summary>
/// Generates CSS custom property declarations (e.g., <c>--color-primary-500: #3B82F6</c>)
/// from a <see cref="DesignTokenSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// This generator translates every token in a <see cref="DesignTokenSet"/> into one or more
/// CSS custom property declarations. The naming convention maps token categories to CSS
/// variable prefixes:
/// </para>
/// <list type="bullet">
///   <item><description>Color tokens → <c>--color-{name}</c> (plus <c>--color-{name}-hover</c>, <c>--color-{name}-active</c>, <c>--color-{name}-foreground</c> when available)</description></item>
///   <item><description>Spacing tokens → <c>--spacing-{name}</c></description></item>
///   <item><description>Typography tokens → <c>--typography-{name}-font-family</c>, <c>--typography-{name}-font-size</c>, <c>--typography-{name}-font-weight</c>, <c>--typography-{name}-line-height</c>, <c>--typography-{name}-letter-spacing</c></description></item>
///   <item><description>Border tokens → <c>--border-{name}-width</c>, <c>--border-{name}-style</c>, <c>--border-{name}-color</c>, <c>--border-{name}-radius</c></description></item>
///   <item><description>Shadow tokens → <c>--shadow-{name}</c></description></item>
///   <item><description>Breakpoint tokens → <c>--breakpoint-{name}</c></description></item>
/// </list>
/// <para>
/// Output is a compact string of semicolon-delimited declarations with a trailing newline.
/// No selector or braces are included — use <see cref="ThemeStyleBuilder"/> to wrap the
/// output in an appropriate scope.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var tokens = DefaultTokens.Create();
/// string css = CssCustomPropertyGenerator.Generate(tokens);
/// // Produces: "--color-gray-50: #F9FAFB; --color-gray-100: #F3F4F6; ..."
/// </code>
/// </example>
public static class CssCustomPropertyGenerator
{
    /// <summary>
    /// Generates CSS custom property declarations from the specified <see cref="DesignTokenSet"/>.
    /// </summary>
    /// <param name="tokens">The design token set to convert. Must not be null.</param>
    /// <returns>A string of CSS custom property declarations, each ending with a semicolon and newline.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="tokens"/> is null.</exception>
    public static string Generate(DesignTokenSet tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);

        var sb = new StringBuilder();

        AppendColorProperties(sb, tokens.Colors);
        AppendSpacingProperties(sb, tokens.Spacing);
        AppendTypographyProperties(sb, tokens.Typography);
        AppendBorderProperties(sb, tokens.Borders);
        AppendShadowProperties(sb, tokens.Shadows);
        AppendBreakpointProperties(sb, tokens.Breakpoints);

        return sb.ToString();
    }

    private static void AppendColorProperties(StringBuilder sb, IReadOnlyDictionary<string, ColorToken> colors)
    {
        foreach (var (key, token) in colors)
        {
            if (!string.IsNullOrWhiteSpace(token.Value))
            {
                sb.Append("  --color-").Append(key).Append(": ").Append(token.Value).AppendLine(";");
            }

            if (!string.IsNullOrWhiteSpace(token.Hover))
            {
                sb.Append("  --color-").Append(key).Append("-hover: ").Append(token.Hover).AppendLine(";");
            }

            if (!string.IsNullOrWhiteSpace(token.Active))
            {
                sb.Append("  --color-").Append(key).Append("-active: ").Append(token.Active).AppendLine(";");
            }

            if (!string.IsNullOrWhiteSpace(token.Foreground))
            {
                sb.Append("  --color-").Append(key).Append("-foreground: ").Append(token.Foreground).AppendLine(";");
            }
        }
    }

    private static void AppendSpacingProperties(StringBuilder sb, IReadOnlyDictionary<string, SpacingToken> spacing)
    {
        foreach (var (_, token) in spacing)
        {
            var name = StripPrefix(token.Name, "spacing-");
            sb.Append("  --spacing-").Append(name).Append(": ").Append(token.Value).AppendLine(";");
        }
    }

    private static void AppendTypographyProperties(StringBuilder sb, IReadOnlyDictionary<string, TypographyToken> typography)
    {
        foreach (var (key, token) in typography)
        {
            sb.Append("  --typography-").Append(key).Append("-font-family: ").Append(token.FontFamily).AppendLine(";");
            sb.Append("  --typography-").Append(key).Append("-font-size: ").Append(token.FontSize).AppendLine(";");
            sb.Append("  --typography-").Append(key).Append("-font-weight: ").Append(token.FontWeight).AppendLine(";");
            sb.Append("  --typography-").Append(key).Append("-line-height: ").Append(token.LineHeight).AppendLine(";");
            sb.Append("  --typography-").Append(key).Append("-letter-spacing: ").Append(token.LetterSpacing).AppendLine(";");
        }
    }

    private static void AppendBorderProperties(StringBuilder sb, IReadOnlyDictionary<string, BorderToken> borders)
    {
        foreach (var (key, token) in borders)
        {
            sb.Append("  --border-").Append(key).Append("-width: ").Append(token.Width).AppendLine(";");
            sb.Append("  --border-").Append(key).Append("-style: ").Append(token.Style).AppendLine(";");
            sb.Append("  --border-").Append(key).Append("-color: ").Append(token.Color).AppendLine(";");
            sb.Append("  --border-").Append(key).Append("-radius: ").Append(token.Radius).AppendLine(";");
        }
    }

    private static void AppendShadowProperties(StringBuilder sb, IReadOnlyDictionary<string, ShadowToken> shadows)
    {
        foreach (var (_, token) in shadows)
        {
            var name = StripPrefix(token.Name, "shadow-");
            sb.Append("  --shadow-").Append(name).Append(": ").Append(token.Value).AppendLine(";");
        }
    }

    private static void AppendBreakpointProperties(StringBuilder sb, IReadOnlyDictionary<string, BreakpointToken> breakpoints)
    {
        foreach (var (key, token) in breakpoints)
        {
            sb.Append("  --breakpoint-").Append(key).Append(": ").Append(token.Value).AppendLine(";");
        }
    }

    private static string StripPrefix(string name, string prefix)
    {
        if (name.StartsWith(prefix, StringComparison.Ordinal))
        {
            return name.Substring(prefix.Length);
        }
        return name;
    }
}
