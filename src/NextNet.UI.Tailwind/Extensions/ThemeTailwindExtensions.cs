using NextNet.UI.Tailwind.Config;
using NextNet.UI.Theming;

namespace NextNet.UI.Tailwind.Extensions;

/// <summary>
/// Provides extension methods for converting <see cref="Theme"/> instances to
/// Tailwind CSS configuration.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ToTailwindConfig"/> is the primary entry point for generating a
/// <see cref="TailwindConfig"/> from a <see cref="Theme"/>. It extracts the
/// theme's <see cref="NextNet.DesignSystem.Tokens.DesignTokenSet"/> and delegates to
/// <see cref="TailwindConfigGenerator.Generate"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// Theme theme = new Theme("light", tokens, metadata);
/// TailwindConfig config = theme.ToTailwindConfig();
/// string jsModule = config.ToJsModuleString();
/// // Use jsModule as the content of tailwind.config.js
/// </code>
/// </example>
public static class ThemeTailwindExtensions
{
    /// <summary>
    /// Converts the specified theme to a <see cref="TailwindConfig"/> by extracting
    /// its design tokens and running them through the Tailwind config generator.
    /// </summary>
    /// <param name="theme">The theme to convert. Must not be null.</param>
    /// <param name="contentPaths">
    /// Optional content paths for the Tailwind config. Defaults to <c>["./**/*.{html,cshtml,razor}"]</c>.
    /// </param>
    /// <param name="safelistPatterns">Optional safelist patterns to include in the Tailwind config.</param>
    /// <returns>A <see cref="TailwindConfig"/> populated from the theme's design tokens.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    public static TailwindConfig ToTailwindConfig(
        this Theme theme,
        IReadOnlyList<string>? contentPaths = null,
        IReadOnlyList<string>? safelistPatterns = null)
    {
        ArgumentNullException.ThrowIfNull(theme);

        return TailwindConfigGenerator.Generate(
            theme.Tokens,
            contentPaths,
            safelistPatterns);
    }
}
