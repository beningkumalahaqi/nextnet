using NextNet.Templates.Versioning;
using Xunit;

namespace NextNet.Templates.Tests.Versioning;

public sealed class TemplateVersionTests
{
    // ============================================================
    // Parse — Valid SemVer strings
    // ============================================================

    [Theory]
    [InlineData("1.0.0")]
    [InlineData("0.0.1")]
    [InlineData("10.20.30")]
    [InlineData("1.0.0-alpha")]
    [InlineData("1.0.0-alpha.1")]
    [InlineData("1.0.0-0.3.7")]
    [InlineData("1.0.0+build.42")]
    [InlineData("1.0.0-alpha+001")]
    [InlineData("2.1.3-alpha.1+build.123")]
    [InlineData("0.0.0")]
    public void Parse_Should_ReturnVersion_When_ValidSemVer(string input)
    {
        var v = TemplateVersion.Parse(input);
        Assert.NotNull(v);
        Assert.Equal(input, v.ToString());
    }

    // ============================================================
    // Parse — Invalid SemVer strings
    // ============================================================

    [Theory]
    [InlineData("")]
    [InlineData("1")]
    [InlineData("1.0")]
    [InlineData("1.0.0.0")]
    [InlineData("v1.0.0")]
    [InlineData("not-a-version")]
    [InlineData("-1.0.0")]
    public void Parse_Should_Throw_When_Invalid(string input)
    {
        Assert.Throws<FormatException>(() => TemplateVersion.Parse(input));
    }

    // ============================================================
    // Properties
    // ============================================================

    [Fact]
    public void Properties_Should_Match_ExpectedValues()
    {
        var v = TemplateVersion.Parse("2.1.3-alpha.1+build.42");

        Assert.Equal(2, v.Major);
        Assert.Equal(1, v.Minor);
        Assert.Equal(3, v.Patch);
        Assert.Equal("alpha.1", v.PreRelease);
        Assert.Equal("build.42", v.BuildMetadata);
        Assert.True(v.IsPreRelease);
    }

    [Fact]
    public void IsPreRelease_Should_BeFalse_When_NoPreRelease()
    {
        var v = TemplateVersion.Parse("1.0.0");
        Assert.False(v.IsPreRelease);
    }

    // ============================================================
    // CompareTo — Numeric ordering
    // ============================================================

    [Fact]
    public void CompareTo_Should_ReturnNegative_When_LesserVersion()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("2.0.0");
        Assert.True(a < b);
        Assert.Equal(-1, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_Should_ReturnPositive_When_GreaterVersion()
    {
        var a = TemplateVersion.Parse("3.0.0");
        var b = TemplateVersion.Parse("2.0.0");
        Assert.True(a > b);
        Assert.Equal(1, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_Should_ReturnZero_When_Equal()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.0.0");
        Assert.Equal(0, a.CompareTo(b));
    }

    [Fact]
    public void CompareTo_Should_OrderByMinor_WhenMajorEqual()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.1.0");
        Assert.True(a < b);
    }

    [Fact]
    public void CompareTo_Should_OrderByPatch_WhenMajorMinorEqual()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.0.1");
        Assert.True(a < b);
    }

    // ============================================================
    // CompareTo — Pre-release rules (SemVer 2.0 §11)
    // ============================================================

    [Fact]
    public void CompareTo_Should_PutPreReleaseBeforeRelease()
    {
        var pre = TemplateVersion.Parse("1.0.0-alpha");
        var rel = TemplateVersion.Parse("1.0.0");
        Assert.True(pre < rel);
    }

    [Fact]
    public void CompareTo_Should_OrderPreReleasesNumerically()
    {
        var a = TemplateVersion.Parse("1.0.0-alpha.1");
        var b = TemplateVersion.Parse("1.0.0-alpha.2");
        Assert.True(a < b);
    }

    [Fact]
    public void CompareTo_Should_OrderPreReleasesLexically()
    {
        var a = TemplateVersion.Parse("1.0.0-alpha");
        var b = TemplateVersion.Parse("1.0.0-beta");
        Assert.True(a < b);
    }

    [Fact]
    public void CompareTo_Should_PreferLongerPreRelease_WhenPrefixesEqual()
    {
        // Per SemVer 2.0: 1.0.0-alpha < 1.0.0-alpha.1
        var a = TemplateVersion.Parse("1.0.0-alpha");
        var b = TemplateVersion.Parse("1.0.0-alpha.1");
        Assert.True(a < b);
    }

    [Fact]
    public void CompareTo_Should_HandleNumericVsAlphaPreRelease()
    {
        // Numeric pre-release < alpha pre-release
        var a = TemplateVersion.Parse("1.0.0-1");
        var b = TemplateVersion.Parse("1.0.0-alpha");
        Assert.True(a < b);
    }

    // ============================================================
    // TryParse
    // ============================================================

    [Fact]
    public void TryParse_Should_ReturnTrue_When_Valid()
    {
        Assert.True(TemplateVersion.TryParse("1.0.0", out var v));
        Assert.NotNull(v);
        Assert.Equal(1, v!.Major);
    }

    [Fact]
    public void TryParse_Should_ReturnFalse_When_Invalid()
    {
        Assert.False(TemplateVersion.TryParse("garbage", out var v));
        Assert.Null(v);
    }

    [Fact]
    public void TryParse_Should_ReturnFalse_When_Null()
    {
        Assert.False(TemplateVersion.TryParse(null, out var v));
        Assert.Null(v);
    }

    // ============================================================
    // ToString — Round-trip
    // ============================================================

    [Fact]
    public void ToString_Should_RoundTrip()
    {
        var v = TemplateVersion.Parse("1.2.3-alpha.1+build.42");
        Assert.Equal("1.2.3-alpha.1+build.42", v.ToString());
    }

    [Fact]
    public void ToString_Should_NotIncludeEmptyPreRelease()
    {
        var v = TemplateVersion.Parse("1.0.0");
        Assert.Equal("1.0.0", v.ToString());
    }

    // ============================================================
    // Equality operators
    // ============================================================

    [Fact]
    public void Equals_Should_ReturnTrue_When_SameVersion()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.0.0");
        Assert.True(a == b);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_Should_ReturnFalse_When_DifferentVersion()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("2.0.0");
        Assert.True(a != b);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void NotEquals_Should_BeTrue_ForDifferentVersions()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.0.1");
        Assert.True(a != b);
    }

    [Fact]
    public void CompareOperators_Should_WorkCorrectly()
    {
        var v1 = TemplateVersion.Parse("1.0.0");
        var v2 = TemplateVersion.Parse("1.5.0");
        var v3 = TemplateVersion.Parse("1.5.0");

        Assert.True(v1 < v2);
        Assert.True(v2 > v1);
        Assert.True(v1 <= v2);
        Assert.True(v2 >= v1);
        Assert.True(v2 >= v3);
        Assert.True(v2 <= v3);
    }

    // ============================================================
    // Constructor validation
    // ============================================================

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Constructor_Should_Throw_When_NegativeComponent(int major, int minor, int patch)
    {
        Assert.Throws<ArgumentException>(() => new TemplateVersion(major, minor, patch));
    }

    // ============================================================
    // GetHashCode — Consistency
    // ============================================================

    [Fact]
    public void GetHashCode_Should_BeConsistent_ForEqualVersions()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("1.0.0");
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_Should_Differ_ForDifferentVersions()
    {
        var a = TemplateVersion.Parse("1.0.0");
        var b = TemplateVersion.Parse("2.0.0");
        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }
}
