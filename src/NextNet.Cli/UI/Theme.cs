using Spectre.Console;

namespace NextNet.Cli.UI;

/// <summary>
/// NextNet color palette and style presets.
/// All colors are defined here; never hardcode hex values in components.
/// </summary>
public static class Theme
{
    // ── Brand ────────────────────────────────────────────────────────
    public const string NextNetTealHex = "#00D4AA";
    public const string VioletHex = "#7C3AED";

    // ── Semantic ─────────────────────────────────────────────────────
    public const string SuccessHex = "#22C55E";
    public const string WarningHex = "#F59E0B";
    public const string ErrorHex = "#EF4444";
    public const string InfoHex = "#3B82F6";
    public const string MutedHex = "#6B7280";
    public const string DimHex = "#4B5563";

    // ── UI ───────────────────────────────────────────────────────────
    public const string SubtleBorderHex = "#374151";
    public const string SubtleBgHex = "#1F2937";

    // ── ANSI 256 fallback values ─────────────────────────────────────
    public const int NextNetTealAnsi = 49;
    public const int VioletAnsi = 134;
    public const int SuccessAnsi = 34;
    public const int WarningAnsi = 214;
    public const int ErrorAnsi = 196;
    public const int InfoAnsi = 63;
    public const int MutedAnsi = 243;
    public const int DimAnsi = 240;
    public const int SubtleBorderAnsi = 238;
    public const int SubtleBgAnsi = 235;

    // ── Parsed Colors ────────────────────────────────────────────────

    /// <summary>NextNet Teal (#00D4AA).</summary>
    public static Color NextNetTeal => new(0x00, 0xD4, 0xAA);

    /// <summary>Violet (#7C3AED).</summary>
    public static Color Violet => new(0x7C, 0x3A, 0xED);

    /// <summary>Success green (#22C55E).</summary>
    public static Color Success => new(0x22, 0xC5, 0x5E);

    /// <summary>Warning amber (#F59E0B).</summary>
    public static Color Warning => new(0xF5, 0x9E, 0x0B);

    /// <summary>Error red (#EF4444).</summary>
    public static Color Error => new(0xEF, 0x44, 0x44);

    /// <summary>Info blue (#3B82F6).</summary>
    public static Color Info => new(0x3B, 0x82, 0xF6);

    /// <summary>Muted gray (#6B7280).</summary>
    public static Color Muted => new(0x6B, 0x72, 0x80);

    /// <summary>Dim gray (#4B5563).</summary>
    public static Color Dim => new(0x4B, 0x55, 0x63);

    /// <summary>Subtle border (#374151).</summary>
    public static Color SubtleBorder => new(0x37, 0x41, 0x51);

    /// <summary>Subtle background (#1F2937).</summary>
    public static Color SubtleBg => new(0x1F, 0x29, 0x37);

    // ── Pre-built Styles ─────────────────────────────────────────────

    /// <summary>Bold teal for headings.</summary>
    public static Style HeadingStyle => new(foreground: NextNetTeal, decoration: Decoration.Bold);

    /// <summary>Muted gray for secondary text.</summary>
    public static Style MutedStyle => new(foreground: Muted);

    /// <summary>Bold green for success states.</summary>
    public static Style SuccessStyle => new(foreground: Success, decoration: Decoration.Bold);

    /// <summary>Bold red for errors.</summary>
    public static Style ErrorStyle => new(foreground: Error, decoration: Decoration.Bold);

    /// <summary>Bold amber for warnings.</summary>
    public static Style WarningStyle => new(foreground: Warning, decoration: Decoration.Bold);

    /// <summary>Bold blue for info.</summary>
    public static Style InfoStyle => new(foreground: Info, decoration: Decoration.Bold);

    /// <summary>Violet for code and links.</summary>
    public static Style CodeStyle => new(foreground: Violet);

    /// <summary>Plain mode fallback — no color, no decoration.</summary>
    public static Style PlainStyle => new(foreground: Color.Default, decoration: Decoration.None);

    /// <summary>Border color for panels and tables.</summary>
    public static Color BorderColor => SubtleBorder;

    /// <summary>Table header color (teal).</summary>
    public static Color TableHeaderColor => NextNetTeal;

    /// <summary>Default style for body text.</summary>
    public static Style DefaultStyle => new(foreground: Color.Default);
}
