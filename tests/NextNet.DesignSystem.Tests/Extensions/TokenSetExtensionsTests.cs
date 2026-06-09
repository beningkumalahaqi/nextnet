using NextNet.DesignSystem.Extensions;
using NextNet.DesignSystem.Tokens;
using Xunit;

namespace NextNet.DesignSystem.Tests.Extensions;

public class TokenSetExtensionsTests
{
    // ─── Merge ───────────────────────────────────────────────────────────────

    [Fact]
    public void Merge_Should_CombineDisjointTokenSets()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            },
            spacing: new Dictionary<string, SpacingToken>
            {
                ["spacing-4"] = new SpacingToken("spacing-4", "1rem")
            });

        var source = new DesignTokenSet(
            typography: new Dictionary<string, TypographyToken>
            {
                ["heading-xl"] = new TypographyToken("heading-xl", "Inter", "1.25rem", "600", "1.75rem", "-0.01em")
            });

        var merged = baseSet.Merge(source);

        Assert.Single(merged.Colors);
        Assert.Single(merged.Spacing);
        Assert.Single(merged.Typography);
    }

    [Fact]
    public void Merge_Should_OverrideExistingKeys_WithSource()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        var source = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#FF0000")
            });

        var merged = baseSet.Merge(source);

        Assert.Equal("#FF0000", merged.Colors["primary-500"].Value);
    }

    [Fact]
    public void Merge_Should_PreserveBaseKeys_WhenSourceDoesNotOverride()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6"),
                ["gray-100"] = new ColorToken("gray-100", "#F3F4F6")
            });

        var source = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#FF0000")
            });

        var merged = baseSet.Merge(source);

        Assert.Equal("#FF0000", merged.Colors["primary-500"].Value);
        Assert.Equal("#F3F4F6", merged.Colors["gray-100"].Value);
    }

    [Fact]
    public void Merge_Should_ReturnNewInstance()
    {
        var baseSet = new DesignTokenSet();
        var source = new DesignTokenSet();
        var merged = baseSet.Merge(source);

        Assert.NotSame(baseSet, merged);
    }

    [Fact]
    public void Merge_Should_ThrowArgumentNullException_When_BaseIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => TokenSetExtensions.Merge(null!, new DesignTokenSet()));
    }

    [Fact]
    public void Merge_Should_ThrowArgumentNullException_When_SourceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DesignTokenSet().Merge(null!));
    }

    // ─── Override ────────────────────────────────────────────────────────────

    [Fact]
    public void Override_Should_ApplyOverrides()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        var overrides = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#FF0000")
                    { Foreground = "#FFFFFF" }
            });

        var result = baseSet.Override(overrides);

        Assert.Equal("#FF0000", result.Colors["primary-500"].Value);
        Assert.Equal("#FFFFFF", result.Colors["primary-500"].Foreground);
    }

    [Fact]
    public void Override_Should_ThrowArgumentNullException_When_BaseIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            TokenSetExtensions.Override(null!, new DesignTokenSet()));
    }

    [Fact]
    public void Override_Should_ThrowArgumentNullException_When_OverridesIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new DesignTokenSet().Override(null!));
    }

    // ─── GetColor ────────────────────────────────────────────────────────────

    [Fact]
    public void GetColor_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        var token = set.GetColor("primary-500");
        Assert.NotNull(token);
        Assert.Equal("#3B82F6", token!.Value);
    }

    [Fact]
    public void GetColor_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetColor("nonexistent"));
    }

    [Fact]
    public void GetColor_Should_ThrowArgumentNullException_When_SetIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => TokenSetExtensions.GetColor(null!, "foo"));
    }

    // ─── GetSpacing ──────────────────────────────────────────────────────────

    [Fact]
    public void GetSpacing_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            spacing: new Dictionary<string, SpacingToken>
            {
                ["spacing-4"] = new SpacingToken("spacing-4", "1rem")
            });

        var token = set.GetSpacing("spacing-4");
        Assert.NotNull(token);
        Assert.Equal("1rem", token!.Value);
    }

    [Fact]
    public void GetSpacing_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetSpacing("nonexistent"));
    }

    [Fact]
    public void GetSpacing_Should_ThrowArgumentNullException_When_SetIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => TokenSetExtensions.GetSpacing(null!, "foo"));
    }

    // ─── GetTypography ──────────────────────────────────────────────────────

    [Fact]
    public void GetTypography_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            typography: new Dictionary<string, TypographyToken>
            {
                ["body-base"] = new TypographyToken("body-base", "Inter", "1rem", "400", "1.5rem", "normal")
            });

        var token = set.GetTypography("body-base");
        Assert.NotNull(token);
        Assert.Equal("1rem", token!.FontSize);
    }

    [Fact]
    public void GetTypography_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetTypography("nonexistent"));
    }

    // ─── GetBorder ───────────────────────────────────────────────────────────

    [Fact]
    public void GetBorder_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            borders: new Dictionary<string, BorderToken>
            {
                ["card"] = new BorderToken("card", "1px", "solid", "#E5E7EB", "0.5rem")
            });

        var token = set.GetBorder("card");
        Assert.NotNull(token);
        Assert.Equal("1px", token!.Width);
    }

    [Fact]
    public void GetBorder_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetBorder("nonexistent"));
    }

    // ─── GetShadow ───────────────────────────────────────────────────────────

    [Fact]
    public void GetShadow_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            shadows: new Dictionary<string, ShadowToken>
            {
                ["shadow-sm"] = new ShadowToken("shadow-sm", "0 1px 2px 0 rgba(0,0,0,0.05)")
            });

        var token = set.GetShadow("shadow-sm");
        Assert.NotNull(token);
        Assert.Equal("0 1px 2px 0 rgba(0,0,0,0.05)", token!.Value);
    }

    [Fact]
    public void GetShadow_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetShadow("nonexistent"));
    }

    // ─── GetBreakpoint ───────────────────────────────────────────────────────

    [Fact]
    public void GetBreakpoint_Should_ReturnToken_When_Found()
    {
        var set = new DesignTokenSet(
            breakpoints: new Dictionary<string, BreakpointToken>
            {
                ["md"] = new BreakpointToken("md", "768px")
            });

        var token = set.GetBreakpoint("md");
        Assert.NotNull(token);
        Assert.Equal("768px", token!.Value);
    }

    [Fact]
    public void GetBreakpoint_Should_ReturnNull_When_NotFound()
    {
        var set = new DesignTokenSet();
        Assert.Null(set.GetBreakpoint("nonexistent"));
    }

    // ─── Merge with Override consistency ────────────────────────────────────

    [Fact]
    public void MergeAndOverride_Should_ProduceSameResult()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        var overrides = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#FF0000")
            });

        var merged = baseSet.Merge(overrides);
        var overridden = baseSet.Override(overrides);

        Assert.Equal(merged, overridden);
    }

    [Fact]
    public void Merge_Should_WorkAcrossAllCategories()
    {
        var baseSet = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken> { ["a"] = new ColorToken("a", "#000") },
            spacing: new Dictionary<string, SpacingToken> { ["x"] = new SpacingToken("x", "1rem") },
            typography: new Dictionary<string, TypographyToken> { ["t"] = new TypographyToken("t", "A", "1rem", "400", "1.5", "normal") },
            borders: new Dictionary<string, BorderToken> { ["b"] = new BorderToken("b", "1px", "solid", "#000", "0") },
            shadows: new Dictionary<string, ShadowToken> { ["s"] = new ShadowToken("s", "0 0 0 #000") },
            breakpoints: new Dictionary<string, BreakpointToken> { ["bp"] = new BreakpointToken("bp", "768px") });

        var source = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken> { ["b"] = new ColorToken("b", "#fff") },
            spacing: new Dictionary<string, SpacingToken> { ["y"] = new SpacingToken("y", "2rem") });

        var result = baseSet.Merge(source);

        Assert.Equal("#000", result.Colors["a"].Value);
        Assert.Equal("#fff", result.Colors["b"].Value);
        Assert.Equal("1rem", result.Spacing["x"].Value);
        Assert.Equal("2rem", result.Spacing["y"].Value);
        Assert.Single(result.Typography);
        Assert.Single(result.Borders);
        Assert.Single(result.Shadows);
        Assert.Single(result.Breakpoints);
    }
}
