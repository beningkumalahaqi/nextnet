using System.Text.RegularExpressions;
using NextNet.Components;
using NextNet.UI.Abstractions.Theme;
using NextNet.UI.Theming;
using NextNet.UI.Theming.Css;
using NextNet.UI.Theming.Presets;

namespace NextNet.UI.Rendering.Head;

/// <summary>
/// Injects theme CSS variables into the <c>&lt;head&gt;</c> section by generating
/// a <c>&lt;style&gt;</c> block with <c>:root</c> CSS custom properties.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeHeadInjector"/> generates the theme CSS that maps the active
/// theme's design tokens (colors, spacing, typography, etc.) to CSS custom properties
/// (variables) on the <c>:root</c> selector. This enables theme-aware styling across
/// all components without runtime class toggling.
/// </para>
/// <para>
/// The injector delegates CSS generation to the <c>NextNet.UI.Theming.Css</c>
/// namespace (specifically <c>ThemeStyleBuilder</c> and
/// <c>CssCustomPropertyGenerator</c>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var injector = new ThemeHeadInjector();
/// var styleContent = injector.Inject("light");
/// // Produces: &lt;style&gt;:root { --color-primary-500: #3B82F6; ... }&lt;/style&gt;
/// </code>
/// </example>
public sealed class ThemeHeadInjector
{
    /// <summary>
    /// Regex pattern that allows only safe characters in theme names
    /// to prevent XSS injection into CSS selectors.
    /// </summary>
    private static readonly Regex SafeThemeNamePattern = new(
        "^[a-zA-Z0-9_-]+$",
        RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(100));

    private readonly IThemeProvider? _themeProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="ThemeHeadInjector"/>
    /// without a theme provider. Theme name lookups will fall back to
    /// default token values.
    /// </summary>
    public ThemeHeadInjector()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ThemeHeadInjector"/> with
    /// an <see cref="IThemeProvider"/> for resolving theme names to
    /// <see cref="Theme"/> objects.
    /// </summary>
    /// <param name="themeProvider">The theme provider used to resolve theme names.</param>
    public ThemeHeadInjector(IThemeProvider? themeProvider)
    {
        _themeProvider = themeProvider;
    }

    /// <summary>
    /// Generates a <c>&lt;style&gt;</c> block containing CSS custom properties
    /// for the specified theme name.
    /// </summary>
    /// <param name="themeName">The name of the theme to inject. If null or empty, the default theme is used.</param>
    /// <returns>
    /// An <see cref="IHtmlContent"/> representing the <c>&lt;style&gt;</c> block,
    /// or <c>null</c> if no theme could be resolved.
    /// </returns>
    /// <remarks>
    /// The generated CSS uses the <c>:root</c> selector so that the variables
    /// are globally available. To use a scoped selector, use
    /// <see cref="Inject(string, CssVariableScope)"/> instead.
    /// </remarks>
    public IHtmlContent? Inject(string? themeName)
    {
        return Inject(themeName, CssVariableScope.Root);
    }

    /// <summary>
    /// Generates a <c>&lt;style&gt;</c> block containing CSS custom properties
    /// for the specified <see cref="Theme"/> and CSS variable scope.
    /// </summary>
    /// <param name="theme">The theme whose tokens will be serialized. Must not be null.</param>
    /// <param name="scope">The <see cref="CssVariableScope"/> controlling the CSS selector wrapping the variables.</param>
    /// <returns>
    /// An <see cref="IHtmlContent"/> representing the <c>&lt;style&gt;</c> block,
    /// or <c>null</c> if <paramref name="theme"/> is null.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    public IHtmlContent? Inject(Theme theme, CssVariableScope scope)
    {
        ArgumentNullException.ThrowIfNull(theme);
        return ThemeStyleBuilder.Build(theme, scope);
    }

    /// <summary>
    /// Generates a <c>&lt;style&gt;</c> block containing CSS custom properties
    /// for the specified theme name and CSS variable scope.
    /// </summary>
    /// <param name="themeName">The name of the theme to inject. If null or empty, the default theme is used.</param>
    /// <param name="scope">The <see cref="CssVariableScope"/> controlling the CSS selector wrapping the variables.</param>
    /// <returns>
    /// An <see cref="IHtmlContent"/> representing the <c>&lt;style&gt;</c> block,
    /// or <c>null</c> if no theme could be resolved.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="scope"/> is an undefined enum value. (Error DS-300)</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="themeName"/> contains invalid characters.</exception>
    public IHtmlContent? Inject(string? themeName, CssVariableScope scope)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            // No theme specified — return the default theme style
            var defaultTheme = LightTheme.Create();
            return ThemeStyleBuilder.Build(defaultTheme, scope);
        }

        if (!Enum.IsDefined(typeof(CssVariableScope), scope))
        {
            throw new ArgumentOutOfRangeException(nameof(scope), scope,
                "DS-300: Undefined CSS variable scope value.");
        }

        // Sanitize: reject theme names with unsafe characters (XSS prevention)
        if (!SafeThemeNamePattern.IsMatch(themeName))
        {
            throw new ArgumentException(
                $"Theme name '{themeName}' contains invalid characters. " +
                "Only alphanumeric characters, underscores, and hyphens are allowed.",
                nameof(themeName));
        }

        // Try to resolve the theme via the provider
        if (_themeProvider != null)
        {
            try
            {
                var tokens = _themeProvider.GetTheme(themeName);
                var fallbackTheme = new Theme(
                    themeName,
                    tokens,
                    new ThemeMetadata(false, themeName, null, null));
                return ThemeStyleBuilder.Build(fallbackTheme, scope);
            }
            catch (KeyNotFoundException)
            {
                // Theme not registered — fall through to fallback
            }
        }

        // Fallback: generate a style block with the proper selector
        // using the default token set
        var defaultTokens = NextNet.DesignSystem.Defaults.DefaultTokens.Create();
        var fallbackThemeWithTokens = new Theme(
            themeName,
            defaultTokens,
            new ThemeMetadata(false, themeName, null, null));
        return ThemeStyleBuilder.Build(fallbackThemeWithTokens, scope);
    }
}
