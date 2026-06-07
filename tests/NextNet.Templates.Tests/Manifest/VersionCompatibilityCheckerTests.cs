using NextNet.Templates.Manifest;
using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Manifest;

public sealed class VersionCompatibilityCheckerTests
{
    private readonly VersionCompatibilityChecker _checker = new();

    private static TemplateManifest CreateManifest(string nextnetVersion) => new(
        Name: "test",
        Version: "1.0.0",
        NextNetVersion: nextnetVersion
    );

    // ============================================================
    // IsCompatible — Exact version match
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_When_ExactVersionMatch()
    {
        // Arrange
        var manifest = CreateManifest("1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 0, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — SDK newer within range
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_When_SdkNewerWithinRange()
    {
        // Arrange
        var manifest = CreateManifest(">=1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(2, 5, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — SDK older
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnFalse_When_SdkOlder()
    {
        // Arrange
        var manifest = CreateManifest(">=2.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 0, 0));

        // Assert
        Assert.False(result.IsCompatible);
        Assert.NotNull(result.Message);
        Assert.Contains("does not satisfy", result.Message, StringComparison.Ordinal);
    }

    // ============================================================
    // IsCompatible — Caret range
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithCaretRange()
    {
        // Arrange
        var manifest = CreateManifest("^1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 5, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — Caret range, major bump
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnFalse_WithCaretRange_MajorBump()
    {
        // Arrange
        var manifest = CreateManifest("^1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(2, 0, 0));

        // Assert
        Assert.False(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — Tilde range
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithTildeRange()
    {
        // Arrange
        var manifest = CreateManifest("~1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 0, 5));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — Tilde range, minor bump
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnFalse_WithTildeRange_MinorBump()
    {
        // Arrange
        var manifest = CreateManifest("~1.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 1, 0));

        // Assert
        Assert.False(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — OR combination
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithOrCombination()
    {
        // Arrange
        var manifest = CreateManifest("1.0.0 || >=2.5.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(3, 0, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — AND combination
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithAndCombination()
    {
        // Arrange
        var manifest = CreateManifest(">=1.0.0 <2.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 5, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — Invalid NextNetVersion
    // ============================================================

    [Fact]
    public void IsCompatible_Should_Throw_When_NextNetVersionIsInvalid()
    {
        // Arrange
        var manifest = CreateManifest("not-a-valid-range");

        // Act & Assert
        Assert.Throws<FormatException>(() => _checker.IsCompatible(manifest, new Version(1, 0, 0)));
    }

    // ============================================================
    // IsCompatible — Null manifest
    // ============================================================

    [Fact]
    public void IsCompatible_Should_Throw_When_ManifestIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _checker.IsCompatible(null!, new Version(1, 0, 0)));
    }

    // ============================================================
    // IsCompatible — Empty NextNetVersion
    // ============================================================

    [Fact]
    public void IsCompatible_Should_Throw_When_NextNetVersionIsEmpty()
    {
        // Arrange
        var manifest = CreateManifest("  ");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _checker.IsCompatible(manifest, new Version(1, 0, 0)));
    }

    // ============================================================
    // ParseRange — Exact version
    // ============================================================

    [Fact]
    public void ParseRange_Should_ParseExactVersion()
    {
        // Act
        var range = _checker.ParseRange("1.0.0");

        // Assert
        Assert.NotNull(range);
        Assert.Single(range.Clauses);
        Assert.Equal("", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 0, 0), range.Clauses[0].Version);
        Assert.Null(range.Alternatives);
    }

    // ============================================================
    // ParseRange — Greater than or equal
    // ============================================================

    [Fact]
    public void ParseRange_Should_ParseGreaterThanOrEqual()
    {
        // Act
        var range = _checker.ParseRange(">=1.5.0");

        // Assert
        Assert.NotNull(range);
        Assert.Single(range.Clauses);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 5, 0), range.Clauses[0].Version);
    }

    // ============================================================
    // ParseRange — OR combination
    // ============================================================

    [Fact]
    public void ParseRange_Should_ParseOrCombination()
    {
        // Act
        var range = _checker.ParseRange("1.0.0 || >=2.0.0");

        // Assert
        Assert.NotNull(range);
        Assert.Single(range.Clauses);
        Assert.Equal("", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 0, 0), range.Clauses[0].Version);

        Assert.NotNull(range.Alternatives);
        Assert.Single(range.Alternatives);
        Assert.Single(range.Alternatives[0].Clauses);
        Assert.Equal(">=", range.Alternatives[0].Clauses[0].Operator);
        Assert.Equal(new Version(2, 0, 0), range.Alternatives[0].Clauses[0].Version);
    }

    // ============================================================
    // ParseRange — AND combination
    // ============================================================

    [Fact]
    public void ParseRange_Should_ParseAndCombination()
    {
        // Act
        var range = _checker.ParseRange(">=1.0.0 <2.0.0");

        // Assert
        Assert.NotNull(range);
        Assert.Equal(2, range.Clauses.Count);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 0, 0), range.Clauses[0].Version);
        Assert.Equal("<", range.Clauses[1].Operator);
        Assert.Equal(new Version(2, 0, 0), range.Clauses[1].Version);
    }

    // ============================================================
    // ParseRange — Caret expansion
    // ============================================================

    [Fact]
    public void ParseRange_Should_ExpandCaret()
    {
        // Act
        var range = _checker.ParseRange("^1.0.0");

        // Assert
        Assert.NotNull(range);
        Assert.Equal(2, range.Clauses.Count);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 0, 0), range.Clauses[0].Version);
        Assert.Equal("<", range.Clauses[1].Operator);
        Assert.Equal(new Version(2, 0, 0), range.Clauses[1].Version);
    }

    // ============================================================
    // ParseRange — Caret expansion with leading zero (0.x)
    // ============================================================

    [Fact]
    public void ParseRange_Should_ExpandCaret_WithLeadingZero()
    {
        // Act
        var range = _checker.ParseRange("^0.1.0");

        // Assert
        Assert.NotNull(range);
        Assert.Equal(2, range.Clauses.Count);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(0, 1, 0), range.Clauses[0].Version);
        Assert.Equal("<", range.Clauses[1].Operator);
        Assert.Equal(new Version(0, 2, 0), range.Clauses[1].Version);
    }

    // ============================================================
    // ParseRange — Caret expansion with 0.0.x
    // ============================================================

    [Fact]
    public void ParseRange_Should_ExpandCaret_WithZeroZero()
    {
        // Act
        var range = _checker.ParseRange("^0.0.3");

        // Assert
        Assert.NotNull(range);
        Assert.Equal(2, range.Clauses.Count);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(0, 0, 3), range.Clauses[0].Version);
        Assert.Equal("<", range.Clauses[1].Operator);
        Assert.Equal(new Version(0, 0, 4), range.Clauses[1].Version);
    }

    // ============================================================
    // ParseRange — Tilde expansion
    // ============================================================

    [Fact]
    public void ParseRange_Should_ExpandTilde()
    {
        // Act
        var range = _checker.ParseRange("~1.0.0");

        // Assert
        Assert.NotNull(range);
        Assert.Equal(2, range.Clauses.Count);
        Assert.Equal(">=", range.Clauses[0].Operator);
        Assert.Equal(new Version(1, 0, 0), range.Clauses[0].Version);
        Assert.Equal("<", range.Clauses[1].Operator);
        Assert.Equal(new Version(1, 1, 0), range.Clauses[1].Version);
    }

    // ============================================================
    // ParseRange — Throw on invalid expression
    // ============================================================

    [Fact]
    public void ParseRange_Should_Throw_When_ExpressionIsInvalid()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _checker.ParseRange(""));
        Assert.Throws<ArgumentException>(() => _checker.ParseRange("   "));
        Assert.Throws<FormatException>(() => _checker.ParseRange(">=abc"));
        Assert.Throws<FormatException>(() => _checker.ParseRange("not-a-range"));
    }

    // ============================================================
    // IsCompatible — Caret range, ^0.x.y
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithCaretZeroMinorRange()
    {
        // Arrange
        var manifest = CreateManifest("^0.1.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(0, 1, 5));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — AND combination returns false
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnFalse_WithAndCombination_WhenOutOfRange()
    {
        // Arrange
        var manifest = CreateManifest(">=1.0.0 <2.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(2, 0, 0));

        // Assert
        Assert.False(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — OR combination, first matches
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnTrue_WithOrCombination_WhenFirstMatches()
    {
        // Arrange
        var manifest = CreateManifest("1.0.0 || >=2.5.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 0, 0));

        // Assert
        Assert.True(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — OR combination, none match
    // ============================================================

    [Fact]
    public void IsCompatible_Should_ReturnFalse_WithOrCombination_WhenNoneMatch()
    {
        // Arrange
        var manifest = CreateManifest("1.0.0 || >=2.5.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(2, 0, 0));

        // Assert
        Assert.False(result.IsCompatible);
    }

    // ============================================================
    // IsCompatible — Well-known message format
    // ============================================================

    [Fact]
    public void IsCompatible_Should_IncludeDetails_When_Incompatible()
    {
        // Arrange
        var manifest = CreateManifest(">=2.0.0");

        // Act
        var result = _checker.IsCompatible(manifest, new Version(1, 0, 0));

        // Assert
        Assert.False(result.IsCompatible);
        Assert.Equal("1.0.0", result.TemplateVersion);
        Assert.Equal(">=2.0.0", result.Constraint);
        Assert.Equal(new Version(1, 0, 0), result.SdkVersion);
    }
}
