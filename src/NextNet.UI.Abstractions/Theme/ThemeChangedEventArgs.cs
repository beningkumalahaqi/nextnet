namespace NextNet.UI.Abstractions.Theme;

/// <summary>
/// Provides data for the <see cref="IThemeProvider.ThemeChanged"/> event.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ThemeChangedEventArgs"/> contains the names of the previous and
/// new themes when a theme change occurs. Consumers can inspect these values
/// to react to theme transitions, such as re-rendering components or updating
/// persisted preferences.
/// </para>
/// </remarks>
/// <param name="OldTheme">The name of the theme before the change.
/// May be <c>null</c> if there was no previously active theme.</param>
/// <param name="NewTheme">The name of the theme after the change.
/// Must not be <c>null</c>.</param>
public sealed record ThemeChangedEventArgs(
    string? OldTheme,
    string NewTheme);
