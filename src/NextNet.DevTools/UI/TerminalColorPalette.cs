namespace NextNet.DevTools.UI;

/// <summary>
/// Maps <see cref="DevToolsColorRole"/> values to <see cref="ConsoleColor"/> and
/// ANSI escape sequences for use in terminal UI rendering.
/// </summary>
/// <remarks>
/// Provides separate dark and light theme mappings. Dark theme uses vibrant
/// colors (Cyan, Green, Yellow, Red) while light theme uses darker variants
/// (Blue, DarkGreen, DarkYellow, DarkRed) for readability on light backgrounds.
///
/// Use <see cref="Resolve(DevToolsColorRole)"/> to get the <see cref="ConsoleColor"/>
/// for a role, or <see cref="FgAnsi(DevToolsColorRole)"/> to get the ANSI foreground
/// escape sequence directly.
/// </remarks>
/// <example>
/// <code>
/// // Dark theme (default terminal appearance)
/// var darkPalette = new TerminalColorPalette(isDark: true);
/// var color = darkPalette.Resolve(DevToolsColorRole.Primary); // ConsoleColor.Cyan
///
/// // Light theme (better readability on white backgrounds)
/// var lightPalette = new TerminalColorPalette(isDark: false);
/// var color = lightPalette.Resolve(DevToolsColorRole.Primary); // ConsoleColor.Blue
/// </code>
/// </example>
public sealed class TerminalColorPalette
{
    private readonly IReadOnlyDictionary<DevToolsColorRole, ConsoleColor> _mapping;

    /// <summary>
    /// Creates a new terminal color palette for either dark or light theme.
    /// </summary>
    /// <param name="isDark">
    /// <c>true</c> for dark theme colors (vibrant on black background),
    /// <c>false</c> for light theme colors (subdued on white background).
    /// </param>
    public TerminalColorPalette(bool isDark)
    {
        _mapping = isDark ? CreateDarkMapping() : CreateLightMapping();
    }

    /// <summary>
    /// Creates a terminal color palette from an explicit role-to-color mapping.
    /// </summary>
    /// <param name="mapping">A dictionary mapping each <see cref="DevToolsColorRole"/>
    /// to its <see cref="ConsoleColor"/>. Must contain all roles.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="mapping"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="mapping"/> is missing required roles.</exception>
    public TerminalColorPalette(IReadOnlyDictionary<DevToolsColorRole, ConsoleColor> mapping)
    {
        ArgumentNullException.ThrowIfNull(mapping);

        var allRoles = Enum.GetValues<DevToolsColorRole>();
        foreach (var role in allRoles)
        {
            if (!mapping.ContainsKey(role))
                throw new ArgumentException($"Missing color mapping for role '{role}'.", nameof(mapping));
        }

        _mapping = mapping;
    }

    /// <summary>
    /// Resolves the <see cref="DevToolsColorRole"/> to its configured <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="role">The semantic color role to resolve.</param>
    /// <returns>The <see cref="ConsoleColor"/> assigned to the specified role.</returns>
    public ConsoleColor Resolve(DevToolsColorRole role) => _mapping[role];

    /// <summary>
    /// Returns the ANSI foreground escape sequence for the specified role.
    /// </summary>
    /// <param name="role">The semantic color role to resolve.</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[36m" for Cyan).</returns>
    public string FgAnsi(DevToolsColorRole role) => AnsiRenderer.Fg(Resolve(role));

    /// <summary>
    /// Returns the ANSI background escape sequence for the specified role.
    /// </summary>
    /// <param name="role">The semantic color role to resolve.</param>
    /// <returns>ANSI escape sequence string (e.g., "\e[46m" for Cyan background).</returns>
    public string BgAnsi(DevToolsColorRole role) => AnsiRenderer.Bg(Resolve(role));

    private static IReadOnlyDictionary<DevToolsColorRole, ConsoleColor> CreateDarkMapping()
    {
        return new Dictionary<DevToolsColorRole, ConsoleColor>
        {
            [DevToolsColorRole.Background] = ConsoleColor.Black,
            [DevToolsColorRole.Foreground] = ConsoleColor.Gray,
            [DevToolsColorRole.Muted] = ConsoleColor.DarkGray,
            [DevToolsColorRole.Primary] = ConsoleColor.Cyan,
            [DevToolsColorRole.PrimaryMuted] = ConsoleColor.DarkCyan,
            [DevToolsColorRole.Danger] = ConsoleColor.Red,
            [DevToolsColorRole.DangerMuted] = ConsoleColor.DarkRed,
            [DevToolsColorRole.Success] = ConsoleColor.Green,
            [DevToolsColorRole.SuccessMuted] = ConsoleColor.DarkGreen,
            [DevToolsColorRole.Warning] = ConsoleColor.Yellow,
            [DevToolsColorRole.WarningMuted] = ConsoleColor.DarkYellow,
            [DevToolsColorRole.Accent] = ConsoleColor.Magenta,
            [DevToolsColorRole.Info] = ConsoleColor.Cyan,
            [DevToolsColorRole.Border] = ConsoleColor.DarkGray,
            [DevToolsColorRole.Highlight] = ConsoleColor.White,
        };
    }

    private static IReadOnlyDictionary<DevToolsColorRole, ConsoleColor> CreateLightMapping()
    {
        return new Dictionary<DevToolsColorRole, ConsoleColor>
        {
            [DevToolsColorRole.Background] = ConsoleColor.White,
            [DevToolsColorRole.Foreground] = ConsoleColor.Black,
            [DevToolsColorRole.Muted] = ConsoleColor.Gray,
            [DevToolsColorRole.Primary] = ConsoleColor.Blue,
            [DevToolsColorRole.PrimaryMuted] = ConsoleColor.DarkBlue,
            [DevToolsColorRole.Danger] = ConsoleColor.DarkRed,
            [DevToolsColorRole.DangerMuted] = ConsoleColor.Red,
            [DevToolsColorRole.Success] = ConsoleColor.DarkGreen,
            [DevToolsColorRole.SuccessMuted] = ConsoleColor.Green,
            [DevToolsColorRole.Warning] = ConsoleColor.DarkYellow,
            [DevToolsColorRole.WarningMuted] = ConsoleColor.Yellow,
            [DevToolsColorRole.Accent] = ConsoleColor.DarkMagenta,
            [DevToolsColorRole.Info] = ConsoleColor.Blue,
            [DevToolsColorRole.Border] = ConsoleColor.Gray,
            [DevToolsColorRole.Highlight] = ConsoleColor.Black,
        };
    }
}
