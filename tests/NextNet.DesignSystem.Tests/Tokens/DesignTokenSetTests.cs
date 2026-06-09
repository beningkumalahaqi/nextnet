using NextNet.DesignSystem.Tokens;
using Xunit;

namespace NextNet.DesignSystem.Tests.Tokens;

public class DesignTokenSetTests
{
    [Fact]
    public void Constructor_Should_InitializeEmptyCollections_When_NoArguments()
    {
        var set = new DesignTokenSet();
        Assert.NotNull(set.Colors);
        Assert.NotNull(set.Spacing);
        Assert.NotNull(set.Typography);
        Assert.NotNull(set.Borders);
        Assert.NotNull(set.Shadows);
        Assert.NotNull(set.Breakpoints);
        Assert.Empty(set.Colors);
        Assert.Empty(set.Spacing);
        Assert.Empty(set.Typography);
        Assert.Empty(set.Borders);
        Assert.Empty(set.Shadows);
        Assert.Empty(set.Breakpoints);
    }

    [Fact]
    public void Constructor_Should_PopulateCollections_When_Provided()
    {
        var colors = new Dictionary<string, ColorToken>
        {
            ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
        };
        var spacing = new Dictionary<string, SpacingToken>
        {
            ["spacing-4"] = new SpacingToken("spacing-4", "1rem")
        };

        var set = new DesignTokenSet(colors: colors, spacing: spacing);

        Assert.Single(set.Colors);
        Assert.Single(set.Spacing);
        Assert.Empty(set.Typography);
        Assert.Empty(set.Borders);
        Assert.Empty(set.Shadows);
        Assert.Empty(set.Breakpoints);
    }

    [Fact]
    public void Constructor_Should_AcceptNull_AsEmpty()
    {
        var set = new DesignTokenSet(
            colors: null,
            spacing: null,
            typography: null,
            borders: null,
            shadows: null,
            breakpoints: null);

        Assert.Empty(set.Colors);
        Assert.Empty(set.Spacing);
        Assert.Empty(set.Typography);
        Assert.Empty(set.Borders);
        Assert.Empty(set.Shadows);
        Assert.Empty(set.Breakpoints);
    }

    [Fact]
    public void Collections_Should_BeImmutable()
    {
        var colors = new Dictionary<string, ColorToken>
        {
            ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
        };
        var set = new DesignTokenSet(colors: colors);

        Assert.IsAssignableFrom<IReadOnlyDictionary<string, ColorToken>>(set.Colors);
    }

    [Fact]
    public void Equality_Should_BeValueBased()
    {
        var colors1 = new Dictionary<string, ColorToken>
        {
            ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
        };
        var colors2 = new Dictionary<string, ColorToken>
        {
            ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
        };

        var set1 = new DesignTokenSet(colors: colors1);
        var set2 = new DesignTokenSet(colors: colors2);

        Assert.Equal(set1, set2);
        Assert.True(set1 == set2);
    }

    [Fact]
    public void Equality_Should_Differ_When_CollectionsDiffer()
    {
        var set1 = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });
        var set2 = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-600"] = new ColorToken("primary-600", "#2563EB")
            });

        Assert.NotEqual(set1, set2);
    }

    [Fact]
    public void NewInstance_Should_BeIndependent()
    {
        var set1 = new DesignTokenSet();
        var set2 = new DesignTokenSet(
            colors: new Dictionary<string, ColorToken>
            {
                ["primary-500"] = new ColorToken("primary-500", "#3B82F6")
            });

        Assert.Empty(set1.Colors);
        Assert.Single(set2.Colors);
    }

    [Fact]
    public void ToString_Should_ContainTypeName()
    {
        var set = new DesignTokenSet();
        var str = set.ToString();
        Assert.NotNull(str);
        Assert.Contains("DesignTokenSet", str);
    }
}
