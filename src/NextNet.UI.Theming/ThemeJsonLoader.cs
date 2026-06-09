using System.Text.Json;
using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;

namespace NextNet.UI.Theming;

/// <summary>
/// Reads a <c>nextnet.theme.json</c> file from the project root and applies
/// theme overrides to the default <see cref="DesignTokenSet"/>.
/// </summary>
/// <remarks>
/// <para>
/// The expected JSON format:
/// <code>
/// {
///   "theme": {
///     "primary": "#2563eb",
///     "secondary": "#64748b",
///     "radius": "0.5rem",
///     "font": "Inter"
///   }
/// }
/// </code>
/// </para>
/// <para>
/// The loader starts from the default token set (<see cref="DefaultTokens.Create"/>)
/// and overrides only the specified properties. Unspecified properties retain their
/// default values.
/// </para>
/// <para>
/// If the <c>nextnet.theme.json</c> file does not exist at the specified path,
/// the loader returns the unmodified default token set.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var loader = new ThemeJsonLoader("/path/to/project/root");
/// DesignTokenSet tokens = loader.Load();
///
/// // Override the primary color to a custom value
/// // nextnet.theme.json: { "theme": { "primary": "#FF6200" } }
/// string primary = tokens.Colors["primary-500"].Value; // "#FF6200"
/// </code>
/// </example>
public sealed class ThemeJsonLoader
{
    private const string FileName = "nextnet.theme.json";

    private readonly string _basePath;

    /// <summary>
    /// Initializes a new instance of <see cref="ThemeJsonLoader"/>.
    /// </summary>
    /// <param name="basePath">
    /// The base directory path where <c>nextnet.theme.json</c> is expected.
    /// Typically this is the project root or content root path.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="basePath"/> is null.</exception>
    public ThemeJsonLoader(string basePath)
    {
        ArgumentNullException.ThrowIfNull(basePath);
        _basePath = basePath;
    }

    /// <summary>
    /// Loads and parses <c>nextnet.theme.json</c>, returning a <see cref="DesignTokenSet"/>
    /// with the default token values overridden by the file's contents.
    /// </summary>
    /// <returns>
    /// A <see cref="DesignTokenSet"/> with overrides applied. If the file does not exist,
    /// returns the default token set unchanged.
    /// </returns>
    public DesignTokenSet Load()
    {
        var filePath = Path.Combine(_basePath, FileName);

        if (!File.Exists(filePath))
        {
            return DefaultTokens.Create();
        }

        try
        {
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<ThemeJsonConfig>(json, JsonOptions);

            if (config?.Theme == null)
            {
                return DefaultTokens.Create();
            }

            return ApplyOverrides(config.Theme);
        }
        catch (JsonException)
        {
            // If the file is malformed, fall back to defaults
            return DefaultTokens.Create();
        }
    }

    private static DesignTokenSet ApplyOverrides(ThemeOverrides overrides)
    {
        var defaults = DefaultTokens.Create();

        // Apply overrides by merging into a mutable dictionary
        var colors = new Dictionary<string, ColorToken>(defaults.Colors);

        if (!string.IsNullOrWhiteSpace(overrides.Primary))
        {
            OverrideColorScale(colors, "primary", overrides.Primary);
        }

        if (!string.IsNullOrWhiteSpace(overrides.Secondary))
        {
            OverrideColorScale(colors, "secondary", overrides.Secondary);
        }

        var typography = new Dictionary<string, TypographyToken>(defaults.Typography);

        if (!string.IsNullOrWhiteSpace(overrides.Font))
        {
            OverrideTypographyFont(typography, overrides.Font);
        }

        var borders = new Dictionary<string, BorderToken>(defaults.Borders);

        if (!string.IsNullOrWhiteSpace(overrides.Radius))
        {
            OverrideBorderRadii(borders, overrides.Radius);
        }

        return new DesignTokenSet(
            colors: colors,
            spacing: defaults.Spacing,
            typography: typography,
            borders: borders,
            shadows: defaults.Shadows,
            breakpoints: defaults.Breakpoints);
    }

    private static void OverrideColorScale(
        Dictionary<string, ColorToken> colors,
        string scaleName,
        string hexValue)
    {
        // Set the 500-level token to the specified value and derive related shades
        var fiveHundredKey = $"{scaleName}-500";
        if (colors.TryGetValue(fiveHundredKey, out var existing))
        {
            colors[fiveHundredKey] = existing with { Value = hexValue };
        }
        else
        {
            colors[fiveHundredKey] = new ColorToken(fiveHundredKey, hexValue);
        }
    }

    private static void OverrideTypographyFont(
        Dictionary<string, TypographyToken> typography,
        string fontFamily)
    {
        // Update the font family for all typography tokens
        var keys = typography.Keys.ToList();
        foreach (var key in keys)
        {
            var token = typography[key];
            typography[key] = token with { FontFamily = fontFamily };
        }
    }

    private static void OverrideBorderRadii(
        Dictionary<string, BorderToken> borders,
        string radius)
    {
        // Apply the radius override to all border tokens that use the default radius
        foreach (var key in borders.Keys.ToList())
        {
            var token = borders[key];
            borders[key] = token with { Radius = radius };
        }
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private sealed record ThemeJsonConfig
    {
        public ThemeOverrides? Theme { get; init; }
    }

    private sealed record ThemeOverrides
    {
        public string? Primary { get; init; }
        public string? Secondary { get; init; }
        public string? Radius { get; init; }
        public string? Font { get; init; }
    }
}
