using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Theming.Presets;

/// <summary>
/// Factory that creates a light-themed <see cref="Theme"/> using the default
/// Tailwind-inspired color palette from <see cref="DefaultTokens"/>.
/// </summary>
/// <remarks>
/// <para>
/// The light theme uses the token set as-is with bright backgrounds and
/// dark text. It is suitable for standard well-lit environments.
/// </para>
/// <para>
/// The theme is named <c>"light"</c> and its metadata indicates it is not a dark variant.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var theme = LightTheme.Create();
/// Console.WriteLine(theme.Name); // "light"
/// Console.WriteLine(theme.Metadata.DisplayName); // "Light"
/// Console.WriteLine(theme.Metadata.IsDark); // False
/// </code>
/// </example>
public static class LightTheme
{
    /// <summary>
    /// Creates a new light-themed <see cref="Theme"/> instance using the default token set.
    /// </summary>
    /// <returns>A fully-populated <see cref="Theme"/> with name <c>"light"</c> and the default token set.</returns>
    public static Theme Create()
    {
        return Create(DefaultTokens.Create());
    }

    /// <summary>
    /// Creates a new light-themed <see cref="Theme"/> instance using the specified base token set.
    /// </summary>
    /// <param name="baseTokens">
    /// The <see cref="DesignTokenSet"/> to use as the base for this theme.
    /// Typically loaded from <c>nextnet.theme.json</c> via <see cref="ThemeJsonLoader"/>.
    /// </param>
    /// <returns>A fully-populated <see cref="Theme"/> with name <c>"light"</c> and the provided token set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseTokens"/> is null.</exception>
    public static Theme Create(DesignTokenSet baseTokens)
    {
        ArgumentNullException.ThrowIfNull(baseTokens);

        var metadata = new ThemeMetadata(
            IsDark: false,
            DisplayName: "Light",
            Description: "A clean, bright theme optimized for well-lit environments",
            IconUrl: null);

        return new Theme("light", baseTokens, metadata);
    }
}
