using NextNet.Configuration;
using NextNet.Edge.Compatibility;
using NextNet.Routing;
using NextNet.Routing.Models;
using Xunit;

namespace NextNet.Edge.Tests.Compatibility;

public class EdgeCompatibilityCheckerTests
{
    private readonly EdgeApiWhitelist _whitelist;
    private readonly EdgeOptions _options;

    public EdgeCompatibilityCheckerTests()
    {
        _whitelist = new EdgeApiWhitelist();
        _options = new EdgeOptions();
    }

    [Fact]
    public void Check_Should_ReturnEmptyReport_When_NoIssues()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var manifest = RouteManifest.Empty;
        var config = new NextNetConfig { Ssg = false, Streaming = false };

        // Act
        var report = checker.Check(manifest, config);

        // Assert
        Assert.False(report.HasViolations);
        Assert.Empty(report.Violations);
    }

    [Fact]
    public void Check_Should_Warn_When_SsgEnabled()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var manifest = RouteManifest.Empty;
        var config = new NextNetConfig { Ssg = true };

        // Act
        var report = checker.Check(manifest, config);

        // Assert
        Assert.True(report.HasViolations);
        Assert.Contains(report.Violations, v =>
            v.Message.Contains("SSG") &&
            v.Severity == EdgeViolationSeverity.Warning);
    }

    [Fact]
    public void Check_Should_ReportInfo_When_MiddlewareRegistered()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var manifest = new RouteManifest(
            Routes: new[] { new RouteEntry("/test", "/test.cshtml", RouteType.Page, RouteSegmentKind.Static) },
            Pages: Array.Empty<RouteEntry>(),
            Layouts: Array.Empty<RouteEntry>(),
            ApiRoutes: Array.Empty<RouteEntry>(),
            ErrorPage: null,
            Conflicts: Array.Empty<RouteConflict>());
        var config = new NextNetConfig();

        // Act
        var report = checker.Check(manifest, config);

        // Assert
        Assert.True(report.HasViolations);
        Assert.Contains(report.Violations, v =>
            v.Severity == EdgeViolationSeverity.Info &&
            v.Message.Contains("Routes are registered"));
    }

    [Fact]
    public void Check_Should_ReportInfo_When_ApiRoutesPresent()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var apiRoute = new RouteEntry("/api/test", "/api/test.cshtml", RouteType.Api, RouteSegmentKind.Static);
        var manifest = new RouteManifest(
            Routes: new[] { apiRoute },
            Pages: Array.Empty<RouteEntry>(),
            Layouts: Array.Empty<RouteEntry>(),
            ApiRoutes: new[] { apiRoute },
            ErrorPage: null,
            Conflicts: Array.Empty<RouteConflict>());
        var config = new NextNetConfig();

        // Act
        var report = checker.Check(manifest, config);

        // Assert
        Assert.True(report.HasViolations);
        Assert.Contains(report.Violations, v =>
            v.Severity == EdgeViolationSeverity.Info &&
            v.Message.Contains("API route"));
    }

    [Fact]
    public void Check_Should_ReportInfo_When_StreamingEnabled()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var manifest = RouteManifest.Empty;
        var config = new NextNetConfig { Streaming = true };

        // Act
        var report = checker.Check(manifest, config);

        // Assert
        Assert.True(report.HasViolations);
        Assert.Contains(report.Violations, v =>
            v.Severity == EdgeViolationSeverity.Info &&
            v.Message.Contains("Streaming"));
    }

    [Fact]
    public void Check_Should_Throw_When_ManifestIsNull()
    {
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        Assert.Throws<ArgumentNullException>(() =>
            checker.Check(null!, new NextNetConfig()));
    }

    [Fact]
    public void Check_Should_Throw_When_ConfigIsNull()
    {
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        Assert.Throws<ArgumentNullException>(() =>
            checker.Check(RouteManifest.Empty, null!));
    }

    [Fact]
    public void CheckType_Should_ReturnViolation_When_TypeIsBlocked()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);

        // Act
        var violation = checker.CheckType("System.IO.File");

        // Assert
        Assert.NotNull(violation);
        Assert.Equal(EdgeViolationSeverity.Warning, violation.Severity);
        Assert.Contains("System.IO.File", violation.Message);
        Assert.NotNull(violation.Suggestion);
    }

    [Fact]
    public void CheckType_Should_ReturnNull_When_TypeIsAllowed()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);

        // Act
        var violation = checker.CheckType("System.Text.Encoding");

        // Assert
        Assert.Null(violation);
    }

    [Fact]
    public void CheckType_Should_ReturnError_When_StrictMode()
    {
        // Arrange
        var strictOptions = new EdgeOptions { Strict = true };
        var checker = new EdgeCompatibilityChecker(_whitelist, strictOptions);

        // Act
        var violation = checker.CheckType("System.IO.File");

        // Assert
        Assert.NotNull(violation);
        Assert.Equal(EdgeViolationSeverity.Error, violation.Severity);
    }

    [Fact]
    public void CheckTypes_Should_ReportAllViolations_When_MultipleBlockedTypes()
    {
        // Arrange
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var types = new[] { "System.IO.File", "System.Text.Json.JsonSerializer", "System.Diagnostics.Process" };

        // Act
        var report = checker.CheckTypes(types);

        // Assert
        Assert.Equal(2, report.Violations.Count); // File and Process are blocked
        Assert.All(report.Violations, v => Assert.Equal(EdgeViolationSeverity.Warning, v.Severity));
    }

    [Fact]
    public void IsAllowedType_Should_ReturnTrue_When_TypeIsAllowed()
    {
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        Assert.True(checker.IsAllowedType("System.Text.Json.JsonSerializer"));
        Assert.False(checker.IsAllowedType("System.IO.File"));
    }

    [Fact]
    public void GetAlternative_Should_ReturnSuggestion_When_TypeIsBlocked()
    {
        var checker = new EdgeCompatibilityChecker(_whitelist, _options);
        var suggestion = checker.GetAlternative("System.IO.File");
        Assert.NotNull(suggestion);
        Assert.Contains("in-memory", suggestion, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Report_Should_CountViolationsCorrectly_When_HasMultiple()
    {
        // Arrange
        var report = new EdgeCompatibilityReport(new[]
        {
            new EdgeViolation(EdgeViolationSeverity.Error, "Error 1"),
            new EdgeViolation(EdgeViolationSeverity.Error, "Error 2"),
            new EdgeViolation(EdgeViolationSeverity.Warning, "Warning 1"),
            new EdgeViolation(EdgeViolationSeverity.Info, "Info 1"),
        });

        // Assert
        Assert.True(report.HasViolations);
        Assert.True(report.HasErrors);
        Assert.Equal(4, report.TotalCount);
        Assert.Equal(2, report.ErrorCount);
        Assert.Equal(1, report.WarningCount);
        Assert.Equal(1, report.InfoCount);
    }

    [Fact]
    public void Report_Should_BeEmpty_When_NoViolations()
    {
        var report = new EdgeCompatibilityReport();
        Assert.False(report.HasViolations);
        Assert.False(report.HasErrors);
        Assert.Equal(0, report.TotalCount);
    }

    [Fact]
    public void Report_Should_Throw_When_ErrorsAndThrowRequested()
    {
        var report = new EdgeCompatibilityReport(new[]
        {
            new EdgeViolation(EdgeViolationSeverity.Error, "Test error"),
        });

        var ex = Assert.Throws<EdgeCompatibilityException>(() => report.ThrowIfErrors());
        Assert.Same(report, ex.Report);
    }

    [Fact]
    public void Report_Should_NotThrow_When_NoErrors()
    {
        var report = new EdgeCompatibilityReport(new[]
        {
            new EdgeViolation(EdgeViolationSeverity.Warning, "Test warning"),
        });

        report.ThrowIfErrors(); // Should not throw
    }

    [Fact]
    public void Violation_Should_IncludeDetails_When_ToStringCalled()
    {
        var violation = new EdgeViolation(
            EdgeViolationSeverity.Error,
            "Test message",
            filePath: "/path/to/file.cs",
            lineNumber: 42,
            typeName: "System.IO.File",
            memberName: "ReadAllText",
            suggestion: "Use MemoryStream instead");

        var str = violation.ToString();
        Assert.Contains("Error", str);
        Assert.Contains("/path/to/file.cs(42)", str);
        Assert.Contains("System.IO.File", str);
        Assert.Contains("Use MemoryStream instead", str);
    }
}
