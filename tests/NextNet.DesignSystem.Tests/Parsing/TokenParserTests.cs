using System.Text.Json;
using NextNet.DesignSystem.Parsing;
using Xunit;

namespace NextNet.DesignSystem.Tests.Parsing;

public class TokenParserTests
{
    [Fact]
    public void Parse_Should_ReturnEmptySet_When_JsonIsEmptyObject()
    {
        var result = TokenParser.Parse("{}", TokenFileFormat.Json);
        Assert.NotNull(result);
        Assert.Empty(result.Colors);
        Assert.Empty(result.Spacing);
        Assert.Empty(result.Typography);
        Assert.Empty(result.Borders);
        Assert.Empty(result.Shadows);
        Assert.Empty(result.Breakpoints);
    }

    [Fact]
    public void Parse_Should_DeserializeColors()
    {
        var json = """
        {
            "colors": {
                "primary-500": { "value": "#3B82F6", "hover": "#2563EB", "active": "#1D4ED8", "foreground": "#FFFFFF" },
                "gray-100": { "value": "#F3F4F6" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Equal(2, result.Colors.Count);
        Assert.Equal("#3B82F6", result.Colors["primary-500"].Value);
        Assert.Equal("#2563EB", result.Colors["primary-500"].Hover);
        Assert.Equal("#1D4ED8", result.Colors["primary-500"].Active);
        Assert.Equal("#FFFFFF", result.Colors["primary-500"].Foreground);
        Assert.Equal("#F3F4F6", result.Colors["gray-100"].Value);
        Assert.Null(result.Colors["gray-100"].Hover);
    }

    [Fact]
    public void Parse_Should_DeserializeSpacing()
    {
        var json = """
        {
            "spacing": {
                "spacing-4": { "value": "1rem" },
                "spacing-8": { "value": "2rem" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Equal(2, result.Spacing.Count);
        Assert.Equal("1rem", result.Spacing["spacing-4"].Value);
        Assert.Equal("2rem", result.Spacing["spacing-8"].Value);
    }

    [Fact]
    public void Parse_Should_DeserializeTypography()
    {
        var json = """
        {
            "typography": {
                "heading-xl": {
                    "fontFamily": "Inter, sans-serif",
                    "fontSize": "1.25rem",
                    "fontWeight": "600",
                    "lineHeight": "1.75rem",
                    "letterSpacing": "-0.01em"
                }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Typography);
        var t = result.Typography["heading-xl"];
        Assert.Equal("Inter, sans-serif", t.FontFamily);
        Assert.Equal("1.25rem", t.FontSize);
        Assert.Equal("600", t.FontWeight);
        Assert.Equal("1.75rem", t.LineHeight);
        Assert.Equal("-0.01em", t.LetterSpacing);
    }

    [Fact]
    public void Parse_Should_DeserializeBorders()
    {
        var json = """
        {
            "borders": {
                "card": { "width": "1px", "style": "solid", "color": "#E5E7EB", "radius": "0.5rem" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Borders);
        var b = result.Borders["card"];
        Assert.Equal("1px", b.Width);
        Assert.Equal("solid", b.Style);
        Assert.Equal("#E5E7EB", b.Color);
        Assert.Equal("0.5rem", b.Radius);
    }

    [Fact]
    public void Parse_Should_DeserializeShadows()
    {
        var json = """
        {
            "shadows": {
                "shadow-sm": { "value": "0 1px 2px 0 rgba(0, 0, 0, 0.05)" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Shadows);
        Assert.Equal("0 1px 2px 0 rgba(0, 0, 0, 0.05)", result.Shadows["shadow-sm"].Value);
    }

    [Fact]
    public void Parse_Should_DeserializeBreakpoints()
    {
        var json = """
        {
            "breakpoints": {
                "md": { "value": "768px" },
                "lg": { "value": "1024px" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Equal(2, result.Breakpoints.Count);
        Assert.Equal("768px", result.Breakpoints["md"].Value);
        Assert.Equal("1024px", result.Breakpoints["lg"].Value);
    }

    [Fact]
    public void Parse_Should_DeserializeAllCategories()
    {
        var json = """
        {
            "colors": { "primary-500": { "value": "#3B82F6" } },
            "spacing": { "spacing-4": { "value": "1rem" } },
            "typography": { "body": { "fontFamily": "Inter", "fontSize": "1rem", "fontWeight": "400", "lineHeight": "1.5rem", "letterSpacing": "normal" } },
            "borders": { "default": { "width": "1px", "style": "solid", "color": "#D1D5DB", "radius": "0" } },
            "shadows": { "shadow-sm": { "value": "0 1px 2px 0 rgba(0,0,0,0.05)" } },
            "breakpoints": { "md": { "value": "768px" } }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Colors);
        Assert.Single(result.Spacing);
        Assert.Single(result.Typography);
        Assert.Single(result.Borders);
        Assert.Single(result.Shadows);
        Assert.Single(result.Breakpoints);
    }

    [Fact]
    public void Parse_Should_ThrowArgumentNullException_When_ContentIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => TokenParser.Parse(null!));
    }

    [Fact]
    public void Parse_Should_ThrowArgumentException_When_ContentIsEmpty()
    {
        Assert.Throws<ArgumentException>(() => TokenParser.Parse(""));
    }

    [Fact]
    public void Parse_Should_ThrowArgumentException_When_ContentIsWhitespace()
    {
        Assert.Throws<ArgumentException>(() => TokenParser.Parse("   "));
    }

    [Fact]
    public void Parse_Should_ThrowJsonException_When_JsonIsMalformed()
    {
        Assert.ThrowsAny<JsonException>(() => TokenParser.Parse("{invalid", TokenFileFormat.Json));
    }

    [Fact]
    public void Parse_Should_ThrowNotSupportedException_When_FormatIsYaml()
    {
        var ex = Assert.Throws<NotSupportedException>(() =>
            TokenParser.Parse("{}", TokenFileFormat.Yaml));
        Assert.Contains("DS-100", ex.Message);
    }

    [Fact]
    public void Parse_Should_IgnoreUnknownCategories()
    {
        var json = """
        {
            "colors": { "primary-500": { "value": "#3B82F6" } },
            "unknown-category": { "foo": { "bar": "baz" } }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Colors);
        Assert.Empty(result.Spacing);
    }

    [Fact]
    public void Parse_Should_IgnoreExtraPropertiesInTokenObject()
    {
        var json = """
        {
            "spacing": {
                "spacing-4": { "value": "1rem", "extraField": "ignored" }
            }
        }
        """;

        var result = TokenParser.Parse(json);

        Assert.Single(result.Spacing);
        Assert.Equal("1rem", result.Spacing["spacing-4"].Value);
    }

    [Fact]
    public void Parse_DefaultFormat_Should_BeJson()
    {
        var result = TokenParser.Parse("{}");
        Assert.NotNull(result);
    }
}
