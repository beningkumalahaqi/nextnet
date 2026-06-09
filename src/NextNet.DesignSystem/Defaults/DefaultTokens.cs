using NextNet.DesignSystem.Tokens;

namespace NextNet.DesignSystem.Defaults;

/// <summary>
/// Factory that creates a Tailwind-inspired default <see cref="DesignTokenSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// The default token set provides a complete set of design tokens suitable for
/// bootstrapping a new project. It includes:
/// </para>
/// <list type="bullet">
///   <item>Color palette with gray, primary, secondary, danger, success, warning, info, purple, and teal scales</item>
///   <item>Spacing scale from 0 to 96 (based on a 0.25rem increment)</item>
///   <item>Typography scale for headings, body, and small text</item>
///   <item>Border tokens for common UI patterns (default, card, input, badge)</item>
///   <item>Shadow tokens for elevation (sm, md, lg, xl, 2xl)</item>
///   <item>Responsive breakpoints (sm: 640px, md: 768px, lg: 1024px, xl: 1280px, 2xl: 1536px)</item>
/// </list>
/// <para>
/// The returned <see cref="DesignTokenSet"/> is immutable. Use
/// <see cref="Extensions.TokenSetExtensions.Merge"/> or
/// <see cref="Extensions.TokenSetExtensions.Override"/> to customize.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var defaults = DefaultTokens.Create();
/// var primaryBlue = defaults.Colors["primary-500"];
/// </code>
/// </example>
public static class DefaultTokens
{
    /// <summary>
    /// Creates the default Tailwind-inspired <see cref="DesignTokenSet"/>.
    /// </summary>
    /// <returns>A fully-populated <see cref="DesignTokenSet"/> with all token categories.</returns>
    public static DesignTokenSet Create()
    {
        return new DesignTokenSet(
            colors: CreateColors(),
            spacing: CreateSpacing(),
            typography: CreateTypography(),
            borders: CreateBorders(),
            shadows: CreateShadows(),
            breakpoints: CreateBreakpoints());
    }

    private static Dictionary<string, ColorToken> CreateColors()
    {
        // Gray scale
        var gray = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var grayValues = new[] { "#F9FAFB", "#F3F4F6", "#E5E7EB", "#D1D5DB", "#9CA3AF", "#6B7280", "#4B5563", "#374151", "#1F2937", "#111827", "#030712" };

        // Blue / Primary
        var blue = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var blueValues = new[] { "#EFF6FF", "#DBEAFE", "#BFDBFE", "#93C5FD", "#60A5FA", "#3B82F6", "#2563EB", "#1D4ED8", "#1E40AF", "#1E3A8A", "#172554" };

        // Red / Danger
        var red = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var redValues = new[] { "#FEF2F2", "#FEE2E2", "#FECACA", "#FCA5A5", "#F87171", "#EF4444", "#DC2626", "#B91C1C", "#991B1B", "#7F1D1D", "#450A0A" };

        // Green / Success
        var green = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var greenValues = new[] { "#F0FDF4", "#DCFCE7", "#BBF7D0", "#86EFAC", "#4ADE80", "#22C55E", "#16A34A", "#15803D", "#166534", "#14532D", "#052E16" };

        // Yellow / Warning
        var yellow = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var yellowValues = new[] { "#FFFBEB", "#FEF3C7", "#FDE68A", "#FCD34D", "#FBBF24", "#F59E0B", "#D97706", "#B45309", "#92400E", "#78350F", "#451A03" };

        // Purple
        var purple = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var purpleValues = new[] { "#FAF5FF", "#F3E8FF", "#E9D5FF", "#D8B4FE", "#C084FC", "#A855F7", "#9333EA", "#7E22CE", "#6B21A8", "#581C87", "#3B0764" };

        // Teal
        var teal = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var tealValues = new[] { "#F0FDFA", "#CCFBF1", "#99F6E4", "#5EEAD4", "#2DD4BF", "#14B8A6", "#0D9488", "#0F766E", "#115E59", "#134E4A", "#042F2E" };

        // Secondary / Slate (blue-gray)
        var secondary = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var secondaryValues = new[] { "#F8FAFC", "#F1F5F9", "#E2E8F0", "#CBD5E1", "#94A3B8", "#64748B", "#475569", "#334155", "#1E293B", "#0F172A", "#020617" };

        // Info / Sky (light blue)
        var info = new[] { "50", "100", "200", "300", "400", "500", "600", "700", "800", "900", "950" };
        var infoValues = new[] { "#F0F9FF", "#E0F2FE", "#BAE6FD", "#7DD3FC", "#38BDF8", "#0EA5E9", "#0284C7", "#0369A1", "#075985", "#0C4A6E", "#082F49" };

        var colors = new Dictionary<string, ColorToken>();
        AddColorRange(colors, "gray", gray, grayValues);
        AddColorRange(colors, "primary", blue, blueValues);
        AddColorRange(colors, "secondary", secondary, secondaryValues);
        AddColorRange(colors, "danger", red, redValues);
        AddColorRange(colors, "success", green, greenValues);
        AddColorRange(colors, "warning", yellow, yellowValues);
        AddColorRange(colors, "info", info, infoValues);
        AddColorRange(colors, "purple", purple, purpleValues);
        AddColorRange(colors, "teal", teal, tealValues);

        // Semantic colors with interactive states
        colors["primary-500"] = colors["primary-500"] with
        {
            Hover = "#2563EB",
            Active = "#1D4ED8",
            Foreground = "#FFFFFF"
        };

        colors["danger-500"] = colors["danger-500"] with
        {
            Hover = "#DC2626",
            Active = "#B91C1C",
            Foreground = "#FFFFFF"
        };

        colors["success-500"] = colors["success-500"] with
        {
            Hover = "#16A34A",
            Active = "#15803D",
            Foreground = "#FFFFFF"
        };

        colors["warning-500"] = colors["warning-500"] with
        {
            Hover = "#D97706",
            Active = "#B45309",
            Foreground = "#FFFFFF"
        };

        colors["secondary-500"] = colors["secondary-500"] with
        {
            Hover = "#475569",
            Active = "#334155",
            Foreground = "#FFFFFF"
        };

        colors["info-500"] = colors["info-500"] with
        {
            Hover = "#0284C7",
            Active = "#0369A1",
            Foreground = "#FFFFFF"
        };

        return colors;
    }

    private static void AddColorRange(Dictionary<string, ColorToken> dict, string prefix, string[] shades, string[] values)
    {
        for (var i = 0; i < shades.Length; i++)
        {
            dict[$"{prefix}-{shades[i]}"] = new ColorToken($"{prefix}-{shades[i]}", values[i]);
        }
    }

    private static Dictionary<string, SpacingToken> CreateSpacing()
    {
        var spacings = new Dictionary<string, SpacingToken>();
        var values = new (string Name, string Value)[]
        {
            ("0", "0px"),
            ("px", "1px"),
            ("0.5", "0.125rem"),
            ("1", "0.25rem"),
            ("1.5", "0.375rem"),
            ("2", "0.5rem"),
            ("2.5", "0.625rem"),
            ("3", "0.75rem"),
            ("3.5", "0.875rem"),
            ("4", "1rem"),
            ("5", "1.25rem"),
            ("6", "1.5rem"),
            ("7", "1.75rem"),
            ("8", "2rem"),
            ("9", "2.25rem"),
            ("10", "2.5rem"),
            ("11", "2.75rem"),
            ("12", "3rem"),
            ("14", "3.5rem"),
            ("16", "4rem"),
            ("20", "5rem"),
            ("24", "6rem"),
            ("28", "7rem"),
            ("32", "8rem"),
            ("36", "9rem"),
            ("40", "10rem"),
            ("44", "11rem"),
            ("48", "12rem"),
            ("52", "13rem"),
            ("56", "14rem"),
            ("60", "15rem"),
            ("64", "16rem"),
            ("72", "18rem"),
            ("80", "20rem"),
            ("96", "24rem"),
        };

        foreach (var (name, value) in values)
        {
            spacings[$"spacing-{name}"] = new SpacingToken($"spacing-{name}", value);
        }

        return spacings;
    }

    private static Dictionary<string, TypographyToken> CreateTypography()
    {
        return new Dictionary<string, TypographyToken>
        {
            ["heading-xs"] = new TypographyToken(
                "heading-xs", "Inter, system-ui, -apple-system, sans-serif",
                "0.75rem", "600", "1rem", "0.01em"),
            ["heading-sm"] = new TypographyToken(
                "heading-sm", "Inter, system-ui, -apple-system, sans-serif",
                "0.875rem", "600", "1.25rem", "0.01em"),
            ["heading-base"] = new TypographyToken(
                "heading-base", "Inter, system-ui, -apple-system, sans-serif",
                "1rem", "600", "1.5rem", "0.01em"),
            ["heading-lg"] = new TypographyToken(
                "heading-lg", "Inter, system-ui, -apple-system, sans-serif",
                "1.125rem", "600", "1.75rem", "-0.01em"),
            ["heading-xl"] = new TypographyToken(
                "heading-xl", "Inter, system-ui, -apple-system, sans-serif",
                "1.25rem", "600", "1.75rem", "-0.01em"),
            ["heading-2xl"] = new TypographyToken(
                "heading-2xl", "Inter, system-ui, -apple-system, sans-serif",
                "1.5rem", "700", "2rem", "-0.01em"),
            ["heading-3xl"] = new TypographyToken(
                "heading-3xl", "Inter, system-ui, -apple-system, sans-serif",
                "1.875rem", "700", "2.25rem", "-0.02em"),
            ["heading-4xl"] = new TypographyToken(
                "heading-4xl", "Inter, system-ui, -apple-system, sans-serif",
                "2.25rem", "700", "2.5rem", "-0.02em"),
            ["body-sm"] = new TypographyToken(
                "body-sm", "Inter, system-ui, -apple-system, sans-serif",
                "0.875rem", "400", "1.25rem", "normal"),
            ["body-base"] = new TypographyToken(
                "body-base", "Inter, system-ui, -apple-system, sans-serif",
                "1rem", "400", "1.5rem", "normal"),
            ["body-lg"] = new TypographyToken(
                "body-lg", "Inter, system-ui, -apple-system, sans-serif",
                "1.125rem", "400", "1.75rem", "normal"),
            ["label"] = new TypographyToken(
                "label", "Inter, system-ui, -apple-system, sans-serif",
                "0.875rem", "500", "1rem", "0.01em"),
            ["caption"] = new TypographyToken(
                "caption", "Inter, system-ui, -apple-system, sans-serif",
                "0.75rem", "400", "1rem", "0.02em"),
            ["overline"] = new TypographyToken(
                "overline", "Inter, system-ui, -apple-system, sans-serif",
                "0.75rem", "600", "1rem", "0.08em"),
        };
    }

    private static Dictionary<string, BorderToken> CreateBorders()
    {
        return new Dictionary<string, BorderToken>
        {
            ["default"] = new BorderToken("default", "1px", "solid", "#E5E7EB", "0"),
            ["card"] = new BorderToken("card", "1px", "solid", "#E5E7EB", "0.5rem"),
            ["input"] = new BorderToken("input", "1px", "solid", "#D1D5DB", "0.375rem"),
            ["badge"] = new BorderToken("badge", "1px", "solid", "transparent", "9999px"),
            ["none"] = new BorderToken("none", "0", "solid", "transparent", "0"),
        };
    }

    private static Dictionary<string, ShadowToken> CreateShadows()
    {
        return new Dictionary<string, ShadowToken>
        {
            ["shadow-sm"] = new ShadowToken("shadow-sm", "0 1px 2px 0 rgba(0, 0, 0, 0.05)"),
            ["shadow-md"] = new ShadowToken("shadow-md", "0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -2px rgba(0, 0, 0, 0.1)"),
            ["shadow-lg"] = new ShadowToken("shadow-lg", "0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -4px rgba(0, 0, 0, 0.1)"),
            ["shadow-xl"] = new ShadowToken("shadow-xl", "0 20px 25px -5px rgba(0, 0, 0, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.1)"),
            ["shadow-2xl"] = new ShadowToken("shadow-2xl", "0 25px 50px -12px rgba(0, 0, 0, 0.25)"),
            ["shadow-inner"] = new ShadowToken("shadow-inner", "inset 0 2px 4px 0 rgba(0, 0, 0, 0.05)"),
        };
    }

    private static Dictionary<string, BreakpointToken> CreateBreakpoints()
    {
        return new Dictionary<string, BreakpointToken>
        {
            ["sm"] = new BreakpointToken("sm", "640px"),
            ["md"] = new BreakpointToken("md", "768px"),
            ["lg"] = new BreakpointToken("lg", "1024px"),
            ["xl"] = new BreakpointToken("xl", "1280px"),
            ["2xl"] = new BreakpointToken("2xl", "1536px"),
        };
    }
}
