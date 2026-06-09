namespace NextNet.UI.Theming;

/// <summary>
/// Resolves the operating system's preferred color scheme (light or dark mode).
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ISystemPreferenceResolver"/> abstracts the detection of the OS-level
/// dark mode preference. The default implementation (<see cref="DefaultSystemPreferenceResolver"/>)
/// returns <c>false</c> (light mode), which acts as a safe fallback. Platform-specific
/// implementations can inspect browser media queries (<c>prefers-color-scheme: dark</c>)
/// or OS-level settings.
/// </para>
/// <para>
/// This resolver is used by <see cref="ThemeManager"/> when the active
/// <see cref="DarkMode"/> is set to <see cref="DarkMode.System"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class BrowserPreferenceResolver : ISystemPreferenceResolver
/// {
///     public bool IsDarkModePreferred() =>
///         /* check navigator.userAgent or OS API */ false;
/// }
/// </code>
/// </example>
public interface ISystemPreferenceResolver
{
    /// <summary>
    /// Returns <c>true</c> if the operating system prefers dark color scheme;
    /// <c>false</c> for light color scheme.
    /// </summary>
    /// <returns><c>true</c> for dark mode; <c>false</c> for light mode.</returns>
    bool IsDarkModePreferred();
}
