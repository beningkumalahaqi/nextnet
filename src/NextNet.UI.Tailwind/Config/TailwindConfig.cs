namespace NextNet.UI.Tailwind.Config;

/// <summary>
/// Represents a serializable Tailwind CSS configuration model matching the
/// <c>tailwind.config.js</c> schema. Used as the intermediate model between
/// NextNet design tokens and the generated Tailwind configuration file.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="TailwindConfig"/> captures the theme extension values derived
/// from a <see cref="NextNet.DesignSystem.Tokens.DesignTokenSet"/>, including colors,
/// spacing, font families, font sizes, and font weights. It also holds
/// content paths and safelist patterns that control Tailwind's content
/// detection and class preservation.
/// </para>
/// <para>
/// Use <see cref="TailwindConfigGenerator.Generate"/> to create an instance
/// from a <see cref="NextNet.DesignSystem.Tokens.DesignTokenSet"/>.
/// Use <see cref="ToJsModuleString"/> to produce the <c>tailwind.config.js</c>
/// file content.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var config = new TailwindConfig
/// {
///     ContentPaths = new[] { "./**/*.{html,cshtml,razor}" },
///     Colors = new Dictionary&lt;string, object&gt; { ["primary"] = new Dictionary&lt;string, string&gt; { ["500"] = "#3B82F6" } },
///     Spacing = new Dictionary&lt;string, string&gt; { ["4"] = "1rem" }
/// };
/// var js = config.ToJsModuleString();
/// </code>
/// </example>
public sealed record TailwindConfig
{
    /// <summary>
    /// Gets or sets the glob file paths that Tailwind should scan for class usage.
    /// Defaults to <c>["./**/*.{html,cshtml,razor}"]</c>.
    /// </summary>
    public IReadOnlyList<string> ContentPaths { get; init; } = new[] { "./**/*.{html,cshtml,razor}" };

    /// <summary>
    /// Gets or sets the list of class patterns that Tailwind should always include
    /// in the generated CSS, even if not detected in content files.
    /// </summary>
    public IReadOnlyList<string> SafelistPatterns { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the extended color palette. Keys are color family names
    /// (e.g., <c>"primary"</c>, <c>"gray"</c>) and values are either hex strings
    /// or nested dictionaries mapping shade numbers to hex values.
    /// </summary>
    public IReadOnlyDictionary<string, object> Colors { get; init; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets the extended spacing scale. Keys are spacing names
    /// (e.g., <c>"4"</c>, <c>"px"</c>) and values are CSS length strings.
    /// </summary>
    public IReadOnlyDictionary<string, string> Spacing { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the extended font family configuration. Keys are family names
    /// (e.g., <c>"sans"</c>, <c>"mono"</c>) and values are comma-separated font stacks.
    /// </summary>
    public IReadOnlyDictionary<string, string> FontFamilies { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the extended font size configuration. Keys are size names
    /// (e.g., <c>"sm"</c>, <c>"base"</c>, <c>"xl"</c>) and values are CSS font-size strings.
    /// </summary>
    public IReadOnlyDictionary<string, string> FontSizes { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the extended font weight configuration. Keys are weight names
    /// (e.g., <c>"normal"</c>, <c>"medium"</c>, <c>"bold"</c>) and values are numeric weight strings.
    /// </summary>
    public IReadOnlyDictionary<string, string> FontWeights { get; init; } = new Dictionary<string, string>();

    /// <summary>
    /// Converts this configuration to a valid JavaScript module string suitable
    /// for use as a <c>tailwind.config.js</c> file.
    /// </summary>
    /// <returns>A JavaScript module string representing this configuration.</returns>
    public string ToJsModuleString()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("export default {");
        sb.AppendLine($"  content: [{string.Join(", ", ContentPaths.Select(p => $"'{p}'"))}],");

        if (SafelistPatterns.Count > 0)
        {
            sb.AppendLine($"  safelist: [{string.Join(", ", SafelistPatterns.Select(p => $"'{p}'"))}],");
        }

        sb.AppendLine("  theme: {");
        sb.AppendLine("    extend: {");

        AppendColors(sb);
        AppendSpacing(sb);
        AppendFontFamilies(sb);
        AppendFontSizes(sb);
        AppendFontWeights(sb);

        sb.AppendLine("    }");
        sb.AppendLine("  }");
        sb.AppendLine("};");

        return sb.ToString();
    }

    private void AppendColors(System.Text.StringBuilder sb)
    {
        if (Colors.Count == 0) return;

        sb.AppendLine("      colors: {");
        foreach (var (family, value) in Colors)
        {
            if (value is string hex)
            {
                sb.AppendLine($"        '{family}': '{hex}',");
            }
            else if (value is IReadOnlyDictionary<string, string> shades)
            {
                sb.AppendLine($"        '{family}': {{");
                foreach (var (shade, color) in shades)
                {
                    sb.AppendLine($"          '{shade}': '{color}',");
                }
                sb.AppendLine($"        }},");
            }
        }
        sb.AppendLine("      },");
    }

    private void AppendSpacing(System.Text.StringBuilder sb)
    {
        if (Spacing.Count == 0) return;

        sb.AppendLine("      spacing: {");
        foreach (var (key, value) in Spacing)
        {
            sb.AppendLine($"        '{key}': '{value}',");
        }
        sb.AppendLine("      },");
    }

    private void AppendFontFamilies(System.Text.StringBuilder sb)
    {
        if (FontFamilies.Count == 0) return;

        sb.AppendLine("      fontFamily: {");
        foreach (var (name, stack) in FontFamilies)
        {
            sb.AppendLine($"        '{name}': '{stack}',");
        }
        sb.AppendLine("      },");
    }

    private void AppendFontSizes(System.Text.StringBuilder sb)
    {
        if (FontSizes.Count == 0) return;

        sb.AppendLine("      fontSize: {");
        foreach (var (name, size) in FontSizes)
        {
            sb.AppendLine($"        '{name}': '{size}',");
        }
        sb.AppendLine("      },");
    }

    private void AppendFontWeights(System.Text.StringBuilder sb)
    {
        if (FontWeights.Count == 0) return;

        sb.AppendLine("      fontWeight: {");
        foreach (var (name, weight) in FontWeights)
        {
            sb.AppendLine($"        '{name}': '{weight}',");
        }
        sb.AppendLine("      },");
    }
}
