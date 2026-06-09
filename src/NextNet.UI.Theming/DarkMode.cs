namespace NextNet.UI.Theming;

/// <summary>
/// Specifies the theme mode for the application.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DarkMode"/> defines three modes:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Light"/> — Always use the light theme.</description></item>
///   <item><description><see cref="Dark"/> — Always use the dark theme.</description></item>
///   <item><description><see cref="System"/> — Follow the operating system's preferred color scheme (light or dark).
///       The resolved theme name is determined by <see cref="ISystemPreferenceResolver"/>.</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var mode = DarkMode.System;
/// // ThemeManager resolves "system" to "light" or "dark" based on OS preference
/// </code>
/// </example>
public enum DarkMode
{
    /// <summary>
    /// Always use the light color scheme.
    /// </summary>
    Light = 0,

    /// <summary>
    /// Always use the dark color scheme.
    /// </summary>
    Dark = 1,

    /// <summary>
    /// Follow the operating system's preferred color scheme.
    /// The resolved theme depends on the <see cref="ISystemPreferenceResolver"/> implementation.
    /// </summary>
    System = 2,
}
