using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Abstractions.Theme;

/// <summary>
/// Defines the contract for a theme provider that supplies design tokens
/// and manages theme state.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="IThemeProvider"/> is the central abstraction for theme management
/// in NextNet. Implementations provide access to the active theme's design tokens,
/// enumerate available themes, and notify consumers of theme changes.
/// </para>
/// <para>
/// The <see cref="GetTheme"/> method retrieves the complete <see cref="DesignTokenSet"/>
/// for a named theme. The <see cref="ActiveTheme"/> property reflects the currently
/// applied theme name.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyThemeProvider : IThemeProvider
/// {
///     private DesignTokenSet _currentTokens = DefaultTokens.Create();
///
///     public string ActiveTheme => "light";
///     public IReadOnlyList&lt;string&gt; AvailableThemes => new[] { "light", "dark" };
///
///     public event EventHandler&lt;ThemeChangedEventArgs&gt;? ThemeChanged;
///
///     public DesignTokenSet GetTheme(string themeName) => _currentTokens;
/// }
/// </code>
/// </example>
public interface IThemeProvider
{
    /// <summary>
    /// Gets the <see cref="DesignTokenSet"/> for the specified theme name.
    /// </summary>
    /// <param name="themeName">The name of the theme to retrieve. Must not be null or empty.</param>
    /// <returns>The <see cref="DesignTokenSet"/> containing all design tokens for the specified theme.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="themeName"/> is null or empty.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified theme is not found.</exception>
    DesignTokenSet GetTheme(string themeName);

    /// <summary>
    /// Gets the name of the currently active theme.
    /// </summary>
    string ActiveTheme { get; }

    /// <summary>
    /// Gets the list of available theme names that can be applied.
    /// </summary>
    IReadOnlyList<string> AvailableThemes { get; }

    /// <summary>
    /// Occurs when the active theme has changed.
    /// </summary>
    /// <remarks>
    /// Subscribers receive a <see cref="ThemeChangedEventArgs"/> instance
    /// containing the old and new theme names.
    /// </remarks>
    event EventHandler<ThemeChangedEventArgs>? ThemeChanged;
}
