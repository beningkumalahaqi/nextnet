namespace NextNet.UI.Theming.Extensions;

/// <summary>
/// Configuration options for the NextNet theming system.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeOptions"/> allows consumers to customize the theme engine's behavior,
/// including the default theme name, dark mode preference, and the set of available themes.
/// These options are typically set during service registration via
/// <see cref="ThemeServiceExtensions.AddNextNetTheming"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddNextNetTheming(options =>
/// {
///     options.DefaultThemeName = "dark";
///     options.DarkMode = DarkMode.System;
///     options.AvailableThemes = new[] { "light", "dark", "high-contrast" };
/// });
/// </code>
/// </example>
public sealed record ThemeOptions
{
    /// <summary>
    /// Gets or sets the name of the default theme used when no explicit theme is requested.
    /// Defaults to <c>"light"</c>.
    /// </summary>
    public string DefaultThemeName { get; set; } = "light";

    /// <summary>
    /// Gets or sets the <see cref="DarkMode"/> for the application.
    /// Defaults to <see cref="DarkMode.Light"/>.
    /// </summary>
    public DarkMode DarkMode { get; set; } = DarkMode.Light;

    /// <summary>
    /// Gets or sets the optional base path for locating <c>nextnet.theme.json</c>.
    /// When set, the <see cref="ThemeJsonLoader"/> will attempt to load theme overrides
    /// from <c>{ThemeJsonBasePath}/nextnet.theme.json</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// If the file does not exist, the default token set is used unchanged.
    /// The typical value is the application's content root path
    /// (e.g., <c>IHostEnvironment.ContentRootPath</c>).
    /// </para>
    /// </remarks>
    public string? ThemeJsonBasePath { get; set; }

    /// <summary>
    /// Gets or sets the list of available theme names for the application.
    /// Defaults to <c>["light", "dark"]</c>.
    /// </summary>
    public IReadOnlyList<string> AvailableThemes { get; set; } = new[] { "light", "dark" };
}
