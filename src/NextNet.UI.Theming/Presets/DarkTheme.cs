using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Theming.Presets;

/// <summary>
/// Factory that creates a dark-themed <see cref="Theme"/> by inverting the default
/// color palette with dark backgrounds and light text.
/// </summary>
/// <remarks>
/// <para>
/// The dark theme inverts the default token set's semantic colors:
/// gray scale is reversed (dark becomes light and vice versa), and semantic colors
/// (primary, secondary, danger, success, warning, info) are adjusted for contrast on dark backgrounds.
/// Typography, spacing, borders, shadows, and breakpoints remain unchanged from the
/// defaults since they are color-independent.
/// </para>
/// <para>
/// The theme is named <c>"dark"</c> and its metadata indicates it is a dark variant.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var theme = DarkTheme.Create();
/// Console.WriteLine(theme.Name); // "dark"
/// Console.WriteLine(theme.Metadata.IsDark); // True
/// </code>
/// </example>
public static class DarkTheme
{
    // Dark-mode gray scale (reversed from light)
    private static readonly Dictionary<string, string> DarkGrayValues = new()
    {
        ["50"] = "#030712",   // maps to gray-950
        ["100"] = "#111827",  // gray-900
        ["200"] = "#1F2937",  // gray-800
        ["300"] = "#374151",  // gray-700
        ["400"] = "#4B5563",  // gray-600
        ["500"] = "#6B7280",  // gray-500 (neutral, unchanged)
        ["600"] = "#9CA3AF",  // gray-400
        ["700"] = "#D1D5DB",  // gray-300
        ["800"] = "#E5E7EB",  // gray-200
        ["900"] = "#F3F4F6",  // gray-100
        ["950"] = "#F9FAFB",  // gray-50
    };

    // Dark-mode semantic color values (scales reversed, 500 shifted lighter)
    private static readonly Dictionary<string, string[]> DarkColorScales = new()
    {
        ["primary"] = new[] { "#172554", "#1E3A8A", "#1E40AF", "#2563EB", "#3B82F6", "#60A5FA", "#93C5FD", "#BFDBFE", "#DBEAFE", "#EFF6FF", "#EFF6FF" },
        ["danger"] = new[] { "#450A0A", "#7F1D1D", "#991B1B", "#B91C1C", "#DC2626", "#F87171", "#FCA5A5", "#FECACA", "#FEE2E2", "#FEF2F2", "#FEF2F2" },
        ["success"] = new[] { "#052E16", "#14532D", "#166534", "#15803D", "#16A34A", "#4ADE80", "#86EFAC", "#BBF7D0", "#DCFCE7", "#F0FDF4", "#F0FDF4" },
        ["warning"] = new[] { "#451A03", "#78350F", "#92400E", "#B45309", "#D97706", "#FBBF24", "#FCD34D", "#FDE68A", "#FEF3C7", "#FFFBEB", "#FFFBEB" },
        ["purple"] = new[] { "#3B0764", "#581C87", "#6B21A8", "#7E22CE", "#9333EA", "#C084FC", "#D8B4FE", "#E9D5FF", "#F3E8FF", "#FAF5FF", "#FAF5FF" },
        ["teal"] = new[] { "#042F2E", "#134E4A", "#115E59", "#0F766E", "#0D9488", "#2DD4BF", "#5EEAD4", "#99F6E4", "#CCFBF1", "#F0FDFA", "#F0FDFA" },
        ["secondary"] = new[] { "#020617", "#0F172A", "#1E293B", "#334155", "#475569", "#64748B", "#94A3B8", "#CBD5E1", "#E2E8F0", "#F1F5F9", "#F8FAFC" },
        ["info"] = new[] { "#082F49", "#0C4A6E", "#075985", "#0369A1", "#0284C7", "#0EA5E9", "#38BDF8", "#7DD3FC", "#BAE6FD", "#E0F2FE", "#F0F9FF" },
    };

    private static readonly string[] Shades = { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };

    /// <summary>
    /// Creates a new dark-themed <see cref="Theme"/> instance using the default token set.
    /// </summary>
    /// <returns>A fully-populated <see cref="Theme"/> with name <c>"dark"</c> and an inverted token set.</returns>
    public static Theme Create()
    {
        return Create(DefaultTokens.Create());
    }

    /// <summary>
    /// Creates a new dark-themed <see cref="Theme"/> instance using the specified base token set.
    /// </summary>
    /// <param name="baseTokens">
    /// The <see cref="DesignTokenSet"/> to use as the base for this dark theme.
    /// Typically loaded from <c>nextnet.theme.json</c> via <see cref="ThemeJsonLoader"/>.
    /// Color tokens are inverted for dark backgrounds.
    /// </param>
    /// <returns>A fully-populated <see cref="Theme"/> with name <c>"dark"</c> and an inverted token set.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseTokens"/> is null.</exception>
    public static Theme Create(DesignTokenSet baseTokens)
    {
        ArgumentNullException.ThrowIfNull(baseTokens);

        var darkColors = CreateDarkColors(baseTokens.Colors);

        var tokens = new DesignTokenSet(
            colors: darkColors,
            spacing: baseTokens.Spacing,
            typography: baseTokens.Typography,
            borders: CreateDarkBorders(baseTokens.Borders),
            shadows: baseTokens.Shadows,
            breakpoints: baseTokens.Breakpoints);

        var metadata = new ThemeMetadata(
            IsDark: true,
            DisplayName: "Dark",
            Description: "A dark theme optimized for low-light environments",
            IconUrl: null);

        return new Theme("dark", tokens, metadata);
    }

    private static Dictionary<string, ColorToken> CreateDarkColors(IReadOnlyDictionary<string, ColorToken> originalColors)
    {
        var colors = new Dictionary<string, ColorToken>(originalColors.Count);

        foreach (var (key, original) in originalColors)
        {
            if (key.StartsWith("gray-"))
            {
                var shade = key.Replace("gray-", "");
                var darkValue = DarkGrayValues.TryGetValue(shade, out var val) ? val : original.Value;
                colors[key] = original with { Value = darkValue };
            }
            else
            {
                // Find which scale this key belongs to
                var scale = DarkColorScales.Keys.FirstOrDefault(s => key.StartsWith($"{s}-"));
                if (scale != null)
                {
                    var shade = key.Replace($"{scale}-", "");
                    var shadeIndex = Array.IndexOf(Shades, shade);
                    var scaleValues = DarkColorScales[scale];

                    if (shadeIndex >= 0 && shadeIndex < scaleValues.Length)
                    {
                        var darkValue = scaleValues[shadeIndex];

                        // For 500-level semantic colors, preserve interactive states
                        if (shade == "500")
                        {
                            colors[key] = new ColorToken(original.Name, darkValue)
                            {
                                Hover = DarkHoverValue(scale),
                                Active = DarkActiveValue(scale),
                                Foreground = "#FFFFFF"
                            };
                        }
                        else
                        {
                            colors[key] = original with { Value = darkValue };
                        }
                    }
                    else
                    {
                        colors[key] = original;
                    }
                }
                else
                {
                    // Pass through unknown colors unchanged
                    colors[key] = original;
                }
            }
        }

        return colors;
    }

    private static string DarkHoverValue(string scaleName) => scaleName switch
    {
        "primary" => "#3B82F6",
        "secondary" => "#64748B",
        "danger" => "#EF4444",
        "success" => "#22C55E",
        "warning" => "#F59E0B",
        "info" => "#0EA5E9",
        _ => "#FFFFFF",
    };

    private static string DarkActiveValue(string scaleName) => scaleName switch
    {
        "primary" => "#2563EB",
        "secondary" => "#475569",
        "danger" => "#DC2626",
        "success" => "#16A34A",
        "warning" => "#D97706",
        "info" => "#0284C7",
        _ => "#FFFFFF",
    };

    private static Dictionary<string, BorderToken> CreateDarkBorders(IReadOnlyDictionary<string, BorderToken> originalBorders)
    {
        var borders = new Dictionary<string, BorderToken>(originalBorders.Count);

        foreach (var (key, original) in originalBorders)
        {
            var adjustedColor = key switch
            {
                "default" => "#4B5563",  // gray-600 on dark bg
                "card" => "#4B5563",
                "input" => "#6B7280",
                "badge" => "transparent",
                "none" => "transparent",
                _ => original.Color
            };

            borders[key] = original with { Color = adjustedColor };
        }

        return borders;
    }
}
