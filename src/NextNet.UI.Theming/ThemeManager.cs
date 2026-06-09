using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Theme;

namespace NextNet.UI.Theming;

/// <summary>
/// Default implementation of <see cref="IThemeProvider"/> that manages theme registration,
/// resolution, and active theme switching with event notifications.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeManager"/> maintains an in-memory dictionary of registered <see cref="Theme"/>
/// instances identified by their <see cref="Theme.Name"/>. Themes are registered via
/// <see cref="RegisterTheme"/> and the active theme is set via <see cref="SetActiveTheme"/>.
/// </para>
/// <para>
/// Support for <see cref="DarkMode.System"/> is provided through the optional
/// <see cref="ISystemPreferenceResolver"/>. When the mode is set to <see cref="DarkMode.System"/>,
/// the active theme is resolved to "light" or "dark" based on the OS preference.
/// Use <see cref="SetDarkMode"/> to set the mode programmatically.
/// </para>
/// <para>
/// When the active theme changes, the <see cref="ThemeChanged"/> event is raised with
/// the old and new theme names. This allows consumers such as the rendering engine
/// or CSS generation pipeline to react to theme transitions.
/// </para>
/// <para>
/// Thread-safety: This implementation is not thread-safe. Synchronize access if
/// themes are modified or switched concurrently.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var manager = new ThemeManager();
/// manager.RegisterTheme(LightTheme.Create());
/// manager.RegisterTheme(DarkTheme.Create());
///
/// // System mode resolves to light or dark based on OS preference
/// manager.SetDarkMode(DarkMode.System);
///
/// manager.SetActiveTheme("dark");
/// var tokens = manager.GetTheme("dark");
///
/// foreach (var name in manager.AvailableThemes)
/// {
///     Console.WriteLine(name);
/// }
/// </code>
/// </example>
public class ThemeManager : IThemeProvider
{
    private readonly Dictionary<string, Theme> _themes = new(StringComparer.OrdinalIgnoreCase);
    private readonly ISystemPreferenceResolver _preferenceResolver;
    private string _activeTheme = string.Empty;
    private DarkMode _mode = DarkMode.Light;

    /// <summary>
    /// Initializes a new instance of the <see cref="ThemeManager"/> class.
    /// </summary>
    /// <param name="preferenceResolver">
    /// An optional <see cref="ISystemPreferenceResolver"/> for detecting OS-level color scheme preference.
    /// If not provided, <see cref="DefaultSystemPreferenceResolver"/> is used, which always reports light mode.
    /// </param>
    public ThemeManager(ISystemPreferenceResolver? preferenceResolver = null)
    {
        _preferenceResolver = preferenceResolver ?? new DefaultSystemPreferenceResolver();
    }

    /// <summary>
    /// Occurs when the active theme has been changed via <see cref="SetActiveTheme"/>
    /// or <see cref="SetDarkMode"/>.
    /// </summary>
    public event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Gets the name of the currently active theme.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="string.Empty"/> if no theme has been set as active yet.
    /// When <see cref="Mode"/> is <see cref="DarkMode.System"/>, this returns the resolved
    /// theme name ("light" or "dark") based on the OS preference.
    /// </remarks>
    public string ActiveTheme => _activeTheme;

    /// <summary>
    /// Gets the current <see cref="DarkMode"/> setting.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="DarkMode.Light"/>. Use <see cref="SetDarkMode"/> to change.
    /// </remarks>
    public DarkMode Mode => _mode;

    /// <summary>
    /// Gets a read-only list of all registered theme names.
    /// </summary>
    public IReadOnlyList<string> AvailableThemes
    {
        get
        {
            lock (_themes)
            {
                return _themes.Keys.ToList().AsReadOnly();
            }
        }
    }

    /// <summary>
    /// Gets the <see cref="DesignTokenSet"/> for the specified theme name.
    /// </summary>
    /// <param name="themeName">The name of the theme to retrieve. Must not be null or empty.</param>
    /// <returns>The <see cref="DesignTokenSet"/> containing all design tokens for the specified theme.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="themeName"/> is null or empty. (Error DS-200)</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified theme is not registered. (Error DS-201)</exception>
    public DesignTokenSet GetTheme(string themeName)
    {
        ValidateThemeName(themeName);
        if (!_themes.TryGetValue(themeName, out var theme))
        {
            throw new KeyNotFoundException($"DS-201: Theme '{themeName}' is not registered.");
        }
        return theme.Tokens;
    }

    /// <summary>
    /// Gets the full <see cref="Theme"/> object for the specified theme name,
    /// including both tokens and metadata.
    /// </summary>
    /// <param name="themeName">The name of the theme to retrieve. Must not be null or empty.</param>
    /// <returns>The <see cref="Theme"/> instance for the specified name.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="themeName"/> is null or empty. (Error DS-202)</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified theme is not registered. (Error DS-203)</exception>
    public Theme GetThemeObject(string themeName)
    {
        ValidateThemeName(themeName);
        if (!_themes.TryGetValue(themeName, out var theme))
        {
            throw new KeyNotFoundException($"DS-203: Theme '{themeName}' is not registered.");
        }
        return theme;
    }

    /// <summary>
    /// Registers a <see cref="Theme"/> with the manager, making it available for resolution
    /// and activation. If a theme with the same name already exists, it is replaced.
    /// </summary>
    /// <param name="theme">The <see cref="Theme"/> instance to register. Must not be null.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="theme"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="theme"/> has a null or empty <see cref="Theme.Name"/>. (Error DS-204)</exception>
    public void RegisterTheme(Theme theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        if (string.IsNullOrWhiteSpace(theme.Name))
        {
            throw new ArgumentException("DS-204: Theme name cannot be null or empty.", nameof(theme));
        }

        _themes[theme.Name] = theme;
    }

    /// <summary>
    /// Sets the active theme to the specified theme name.
    /// </summary>
    /// <param name="themeName">The name of the theme to activate. Must not be null or empty.
    /// The value <c>"system"</c> is treated as <see cref="DarkMode.System"/> and resolved
    /// via the <see cref="ISystemPreferenceResolver"/>.</param>
    /// <returns><c>true</c> if the active theme was changed; <c>false</c> if the theme is not registered.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="themeName"/> is null or empty. (Error DS-205)</exception>
    public bool SetActiveTheme(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            throw new ArgumentException("DS-205: Theme name cannot be null or empty.", nameof(themeName));
        }

        // Treat "system" as DarkMode.System
        if (string.Equals(themeName, "system", StringComparison.OrdinalIgnoreCase))
        {
            SetDarkMode(DarkMode.System);
            return true;
        }

        if (!_themes.ContainsKey(themeName))
        {
            return false;
        }

        if (string.Equals(_activeTheme, themeName, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var oldTheme = _activeTheme;
        _activeTheme = themeName;
        _mode = DarkMode.Light; // Explicit theme name overrides mode
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(
            string.IsNullOrEmpty(oldTheme) ? null : oldTheme,
            themeName));
        return true;
    }

    /// <summary>
    /// Sets the <see cref="DarkMode"/> for theme resolution.
    /// </summary>
    /// <param name="mode">The <see cref="DarkMode"/> to apply.</param>
    /// <remarks>
    /// <para>
    /// When set to <see cref="DarkMode.System"/>, the active theme is resolved to "light" or "dark"
    /// based on the configured <see cref="ISystemPreferenceResolver"/>. When set to
    /// <see cref="DarkMode.Light"/> or <see cref="DarkMode.Dark"/>, the corresponding theme
    /// is activated directly.
    /// </para>
    /// <para>
    /// If the resolved theme name is not registered, this method returns <c>false</c> and
    /// no change occurs.
    /// </para>
    /// </remarks>
    /// <returns><c>true</c> if the active theme was changed; <c>false</c> if the resolved theme is not registered.</returns>
    public bool SetDarkMode(DarkMode mode)
    {
        _mode = mode;

        var resolvedTheme = mode switch
        {
            DarkMode.Light => "light",
            DarkMode.Dark => "dark",
            DarkMode.System => _preferenceResolver.IsDarkModePreferred() ? "dark" : "light",
            _ => "light"
        };

        // Only activate if the resolved theme exists
        if (!_themes.ContainsKey(resolvedTheme))
        {
            return false;
        }

        if (string.Equals(_activeTheme, resolvedTheme, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var oldTheme = _activeTheme;
        _activeTheme = resolvedTheme;
        ThemeChanged?.Invoke(this, new ThemeChangedEventArgs(
            string.IsNullOrEmpty(oldTheme) ? null : oldTheme,
            resolvedTheme));
        return true;
    }

    private static void ValidateThemeName(string themeName)
    {
        if (string.IsNullOrWhiteSpace(themeName))
        {
            throw new ArgumentException("DS-200: Theme name cannot be null or empty.", nameof(themeName));
        }
    }
}
