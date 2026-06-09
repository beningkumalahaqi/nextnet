namespace NextNet.DevTools.UI;

/// <summary>
/// Defines semantic color roles used by the DevTools terminal theming system.
/// Each role maps to a <see cref="ConsoleColor"/> and an ANSI escape sequence
/// via <see cref="TerminalColorPalette"/>.
/// </summary>
/// <remarks>
/// Roles are grouped into primary semantic colors and their muted variants.
/// Use <see cref="TerminalColorPalette.Resolve(DevToolsColorRole)"/> to get the
/// concrete <see cref="ConsoleColor"/> for a role in the active theme.
/// </remarks>
/// <example>
/// <code>
/// var palette = new TerminalColorPalette(isDark: true);
/// var color = palette.Resolve(DevToolsColorRole.Primary); // ConsoleColor.Cyan
/// var ansi  = palette.FgAnsi(DevToolsColorRole.Primary);  // "\e[36m"
/// </code>
/// </example>
public enum DevToolsColorRole
{
    /// <summary>Default background color of the terminal.</summary>
    Background,

    /// <summary>Default foreground / text color.</summary>
    Foreground,

    /// <summary>Muted / secondary text color.</summary>
    Muted,

    /// <summary>Primary accent color (e.g., key information, headings).</summary>
    Primary,

    /// <summary>Muted variant of the primary accent color.</summary>
    PrimaryMuted,

    /// <summary>Error / danger / destructive actions.</summary>
    Danger,

    /// <summary>Muted variant of the danger color.</summary>
    DangerMuted,

    /// <summary>Success / positive actions.</summary>
    Success,

    /// <summary>Muted variant of the success color.</summary>
    SuccessMuted,

    /// <summary>Warning / caution.</summary>
    Warning,

    /// <summary>Muted variant of the warning color.</summary>
    WarningMuted,

    /// <summary>Accent / highlight color distinct from primary (e.g., for links).</summary>
    Accent,

    /// <summary>Informational / neutral highlight.</summary>
    Info,

    /// <summary>Border and separator lines.</summary>
    Border,

    /// <summary>Text highlight / selection background.</summary>
    Highlight
}
