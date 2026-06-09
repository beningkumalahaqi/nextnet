using System.Text.Json;
using NextNet.DesignSystem.Tokens;

namespace NextNet.DesignSystem.Parsing;

/// <summary>
/// Static parser that deserializes token definition strings into <see cref="DesignTokenSet"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The expected JSON structure is a flat or nested map of token categories. Each category
/// contains token objects keyed by name. The parser supports six categories:
/// <c>colors</c>, <c>spacing</c>, <c>typography</c>, <c>borders</c>, <c>shadows</c>, and <c>breakpoints</c>.
/// </para>
/// <para>
/// Unrecognized properties in the JSON are silently ignored, making the parser forward-compatible
/// with future token types.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var json = """
/// {
///   "colors": {
///     "primary-500": { "value": "#3B82F6", "hover": "#2563EB" }
///   },
///   "spacing": {
///     "spacing-4": { "value": "1rem" }
///   }
/// }
/// """;
/// var tokens = TokenParser.Parse(json, TokenFileFormat.Json);
/// </code>
/// </example>
public static class TokenParser
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };
    /// <summary>
    /// Parses the specified token definition <paramref name="content"/> into a <see cref="DesignTokenSet"/>.
    /// </summary>
    /// <param name="content">The raw string content to parse.</param>
    /// <param name="format">The format of the content. Defaults to <see cref="TokenFileFormat.Json"/>.</param>
    /// <returns>A populated <see cref="DesignTokenSet"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="content"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="content"/> is empty or whitespace.</exception>
    /// <exception cref="NotSupportedException">Thrown if <paramref name="format"/> is not supported (e.g., <see cref="TokenFileFormat.Yaml"/>).</exception>
    /// <exception cref="JsonException">Thrown if the JSON content is malformed or type-incompatible.</exception>
    public static DesignTokenSet Parse(string content, TokenFileFormat format = TokenFileFormat.Json)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Token content must not be empty or whitespace.", nameof(content));
        }

        return format switch
        {
            TokenFileFormat.Json => ParseJson(content),
            TokenFileFormat.Yaml => throw new NotSupportedException(
                "YAML format is not yet supported. (DS-100) Use Json format instead."),
            _ => throw new NotSupportedException(
                $"Unknown token file format: {format}. (DS-101)")
        };
    }

    private static DesignTokenSet ParseJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var colors = ParseCategory<ColorTokenData>(root, "colors")
            .ToDictionary(kvp => kvp.Key, kvp => CreateColorToken(kvp.Key, kvp.Value));

        var spacing = ParseCategory<SpacingTokenData>(root, "spacing")
            .ToDictionary(kvp => kvp.Key, kvp => CreateSpacingToken(kvp.Key, kvp.Value));

        var typography = ParseCategory<TypographyTokenData>(root, "typography")
            .ToDictionary(kvp => kvp.Key, kvp => CreateTypographyToken(kvp.Key, kvp.Value));

        var borders = ParseCategory<BorderTokenData>(root, "borders")
            .ToDictionary(kvp => kvp.Key, kvp => CreateBorderToken(kvp.Key, kvp.Value));

        var shadows = ParseCategory<ShadowTokenData>(root, "shadows")
            .ToDictionary(kvp => kvp.Key, kvp => new ShadowToken(kvp.Key, kvp.Value.Value));

        var breakpoints = ParseCategory<BreakpointTokenData>(root, "breakpoints")
            .ToDictionary(kvp => kvp.Key, kvp => new BreakpointToken(kvp.Key, kvp.Value.Value));

        return new DesignTokenSet(
            colors: colors,
            spacing: spacing,
            typography: typography,
            borders: borders,
            shadows: shadows,
            breakpoints: breakpoints);
    }

    private static IEnumerable<KeyValuePair<string, T>> ParseCategory<T>(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var categoryEl))
        {
            yield break;
        }

        foreach (var token in categoryEl.EnumerateObject())
        {
            var data = JsonSerializer.Deserialize<T>(token.Value.GetRawText(), JsonOptions);
            if (data is not null)
            {
                yield return new KeyValuePair<string, T>(token.Name, data);
            }
        }
    }

    private static ColorToken CreateColorToken(string name, ColorTokenData data)
    {
        var token = new ColorToken(name, data.Value);
        if (data.Hover is not null) token = token with { Hover = data.Hover };
        if (data.Active is not null) token = token with { Active = data.Active };
        if (data.Foreground is not null) token = token with { Foreground = data.Foreground };
        return token;
    }

    private static SpacingToken CreateSpacingToken(string name, SpacingTokenData data)
        => new(name, data.Value);

    private static TypographyToken CreateTypographyToken(string name, TypographyTokenData data)
        => new(name, data.FontFamily, data.FontSize, data.FontWeight, data.LineHeight, data.LetterSpacing);

    private static BorderToken CreateBorderToken(string name, BorderTokenData data)
        => new(name, data.Width, data.Style, data.Color, data.Radius);

    // ─── JSON DTOs ───────────────────────────────────────────────────────────

    private sealed record ColorTokenData(string Value, string? Hover, string? Active, string? Foreground);
    private sealed record SpacingTokenData(string Value);
    private sealed record TypographyTokenData(
        string FontFamily,
        string FontSize,
        string FontWeight,
        string LineHeight,
        string LetterSpacing);
    private sealed record BorderTokenData(string Width, string Style, string Color, string Radius);
    private sealed record ShadowTokenData(string Value);
    private sealed record BreakpointTokenData(string Value);
}
