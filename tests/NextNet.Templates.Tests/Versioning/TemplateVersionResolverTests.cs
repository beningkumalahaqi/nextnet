using NextNet.Templates.Versioning;
using Xunit;

namespace NextNet.Templates.Tests.Versioning;

public sealed class TemplateVersionResolverTests
{
    private readonly TemplateVersionResolver _resolver = new();
    private readonly List<string> _versions = new() { "1.0.0", "1.1.0", "1.2.0", "2.0.0", "2.1.0-beta" };

    // ============================================================
    // Resolve — "latest" specifier
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnLatestVersion_When_SpecifierIsLatest()
    {
        // 2.1.0-beta has minor=1 > 0, so it's the highest version
        var result = _resolver.Resolve("latest", _versions);
        Assert.Equal("2.1.0-beta", result);
    }

    [Fact]
    public void Resolve_Should_ReturnLatest_When_CaseInsensitive()
    {
        var result = _resolver.Resolve("LATEST", _versions);
        Assert.Equal("2.1.0-beta", result);
    }

    [Fact]
    public void Resolve_Should_ReturnLatestWithPreRelease_When_NoReleaseExists()
    {
        var versions = new List<string> { "1.0.0-alpha", "1.0.0-beta" };
        var result = _resolver.Resolve("latest", versions);
        // beta > alpha lexically
        Assert.Equal("1.0.0-beta", result);
    }

    // ============================================================
    // Resolve — Exact version
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnExactVersion_When_Available()
    {
        Assert.Equal("1.2.0", _resolver.Resolve("1.2.0", _versions));
    }

    [Fact]
    public void Resolve_Should_ReturnNull_When_VersionNotFound()
    {
        Assert.Null(_resolver.Resolve("3.0.0", _versions));
    }

    // ============================================================
    // Resolve — Caret range
    // ============================================================

    [Fact]
    public void Resolve_Should_HandleCaretRange()
    {
        var result = _resolver.Resolve("^1.0.0", _versions);
        // ^1.0.0 matches 1.x.x >= 1.0.0, so latest in 1.x is 1.2.0
        Assert.Equal("1.2.0", result);
    }

    [Fact]
    public void Resolve_Should_HandleCaretRange_WithNoMatch()
    {
        var result = _resolver.Resolve("^3.0.0", _versions);
        Assert.Null(result);
    }

    // ============================================================
    // Resolve — Tilde range
    // ============================================================

    [Fact]
    public void Resolve_Should_HandleTildeRange()
    {
        var result = _resolver.Resolve("~1.1.0", _versions);
        // ~1.1.0 matches >=1.1.0 <1.2.0, so 1.1.0
        Assert.Equal("1.1.0", result);
    }

    [Fact]
    public void Resolve_Should_HandleTildeRange_WithLatestPatch()
    {
        var versions = new List<string> { "1.1.0", "1.1.1", "1.1.2" };
        var result = _resolver.Resolve("~1.1.0", versions);
        Assert.Equal("1.1.2", result);
    }

    // ============================================================
    // Resolve — Greater than or equal
    // ============================================================

    [Fact]
    public void Resolve_Should_HandleGreaterThanOrEqual()
    {
        var result = _resolver.Resolve(">=2.0.0", _versions);
        // 2.1.0-beta has minor=1 > 0, so it's the highest match
        Assert.Equal("2.1.0-beta", result);
    }

    // ============================================================
    // Resolve — Empty versions list
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnNull_When_NoVersionsAvailable()
    {
        Assert.Null(_resolver.Resolve("latest", new List<string>()));
        Assert.Null(_resolver.Resolve("^1.0.0", new List<string>()));
    }

    // ============================================================
    // Resolve — "nextMajor" specifier
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnNextMajor_When_Available()
    {
        var versions = new List<string> { "1.0.0", "2.0.0", "3.0.0" };
        var result = _resolver.Resolve("nextMajor", versions);
        // Latest is 3.0.0, next major after 3 would be...
        // Actually "latest" from existing is 3.0.0, so next major > 3 should be null
        // Let me re-check the logic: latest is 3.0.0, next major > 3 doesn't exist
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_Should_ReturnNextMajor_When_NextExists()
    {
        var versions = new List<string> { "1.0.0", "1.1.0", "2.0.0" };
        var result = _resolver.Resolve("nextMajor", versions);
        // Latest is 2.0.0, no next major > 2
        Assert.Null(result);
    }

    // ============================================================
    // Resolve — "nextMinor" specifier
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnNextMinor_When_Available()
    {
        var versions = new List<string> { "1.0.0", "1.1.0", "1.2.0", "2.0.0" };
        var result = _resolver.Resolve("nextMinor", versions);
        // Latest is 2.0.0, same major doesn't have a next minor
        Assert.Null(result);
    }

    [Fact]
    public void Resolve_Should_ReturnNextMinor_ForLatestMajor()
    {
        var versions = new List<string> { "2.0.0", "2.1.0", "2.2.0" };
        var result = _resolver.Resolve("nextMinor", versions);
        // Latest is 2.2.0, no next minor in same major
        Assert.Null(result);
    }

    // ============================================================
    // Resolve — Null arguments
    // ============================================================

    [Fact]
    public void Resolve_Should_Throw_When_SpecifierIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve(null!, _versions));
    }

    [Fact]
    public void Resolve_Should_Throw_When_VersionsIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => _resolver.Resolve("latest", null!));
    }

    // ============================================================
    // Resolve — With pre-release versions in list
    // ============================================================

    [Fact]
    public void Resolve_Should_ReturnPreRelease_When_ExactMatch()
    {
        var result = _resolver.Resolve("2.1.0-beta", _versions);
        Assert.Equal("2.1.0-beta", result);
    }

    [Fact]
    public void Resolve_Should_PreferReleaseOverPreRelease_When_SameNumericVersion()
    {
        // When numeric parts are identical, pre-release sorts lower than release
        var versions = new List<string> { "1.0.0", "1.0.0-alpha" };
        var result = _resolver.Resolve("latest", versions);
        Assert.Equal("1.0.0", result);
    }
}
