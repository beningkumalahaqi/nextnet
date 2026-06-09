namespace NextNet.UI.Theming;

/// <summary>
/// Default implementation of <see cref="ISystemPreferenceResolver"/> that always
/// reports light mode as the preferred color scheme.
/// </summary>
/// <remarks>
/// <para>
/// This implementation serves as a safe fallback for environments where OS-level
/// preference detection is unavailable (e.g., server-side rendering, console apps).
/// Platform-specific implementations (e.g., Blazor, MAUI, ASP.NET Core middleware)
/// should inspect <c>prefers-color-scheme</c> media queries or OS-level settings.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var resolver = new DefaultSystemPreferenceResolver();
/// bool isDark = resolver.IsDarkModePreferred(); // Always false
/// </code>
/// </example>
public sealed class DefaultSystemPreferenceResolver : ISystemPreferenceResolver
{
    /// <summary>
    /// Always returns <c>false</c> (light mode).
    /// </summary>
    /// <returns><c>false</c> — indicates light mode is preferred.</returns>
    public bool IsDarkModePreferred() => false;
}
