using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Manifest;
using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Manifest;

public sealed class ManifestValidatorTests
{
    private readonly ManifestValidator _validator = new();

    private static TemplateManifest CreateValidManifest() => new(
        Name: "my-template",
        Version: "1.0.0",
        NextNetVersion: ">=3.0.0",
        Author: "TestAuthor",
        Description: "A test template",
        Tags: new[] { "test" },
        Variables: new[]
        {
            new TemplateVariable("projectName", "string", Description: "Project name", Required: true),
            new TemplateVariable("framework", "string", Description: "Target framework")
        },
        Features: new[]
        {
            new TemplateFeature("auth", "Authentication"),
            new TemplateFeature("logging", "Logging")
        },
        Files: new[]
        {
            new TemplateFile("Program.cs", "Program.cs"),
            new TemplateFile("Startup.cs", "Startup.cs")
        }
    );

    // ============================================================
    // Validate — Success
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnSuccess_When_ManifestIsValid()
    {
        // Arrange
        var manifest = CreateValidManifest();

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Errors);
    }

    // ============================================================
    // Validate — Name empty
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_NameIsEmpty()
    {
        // Arrange
        var manifest = CreateValidManifest() with { Name = "" };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("name", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Name invalid characters
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_NameHasInvalidCharacters()
    {
        // Arrange
        var manifest = CreateValidManifest() with { Name = "my template!" };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("invalid characters", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Version not SemVer
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_VersionIsNotSemVer()
    {
        // Arrange
        var manifest = CreateValidManifest() with { Version = "not-valid" };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("SemVer", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — NextNetVersion invalid range
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_NextNetVersionIsInvalid()
    {
        // Arrange
        var manifest = CreateValidManifest() with { NextNetVersion = "not-a-range" };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("NextNet", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — File source path empty
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_FileSourcePathIsEmpty()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Files = new[]
            {
                new TemplateFile("", "Target.cs")
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("source", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — File target path empty
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_FileTargetPathIsEmpty()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Files = new[]
            {
                new TemplateFile("Source.cs", "")
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("target", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Duplicate variable names
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_VariableNamesAreDuplicated()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Variables = new[]
            {
                new TemplateVariable("dupName", "string"),
                new TemplateVariable("dupName", "string")
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Duplicate feature names
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_FeatureNamesAreDuplicated()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Features = new[]
            {
                new TemplateFeature("dupFeature"),
                new TemplateFeature("dupFeature")
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("duplicate", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Feature depends on undefined feature
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_FeatureDependsOnUndefinedFeature()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Features = new[]
            {
                new TemplateFeature("featureA", Dependencies: new[] { "undefinedFeature" })
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("undefined", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Feature conflicts with undefined feature
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_FeatureConflictsWithUndefinedFeature()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Features = new[]
            {
                new TemplateFeature("featureA", Conflicts: new[] { "undefinedConflict" })
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("undefined", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Enum variable has no allowed values
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnError_When_EnumVariableHasNoAllowedValues()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Variables = new[]
            {
                new TemplateVariable("enumVar", "enum")
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("allowed", StringComparison.OrdinalIgnoreCase));
    }

    // ============================================================
    // Validate — Multiple issues
    // ============================================================

    [Fact]
    public void Validate_Should_ReturnMultipleErrors_When_ManifestHasMultipleIssues()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Name = "",                                    // error 1
            Version = "bad",                              // error 2
            NextNetVersion = "invalid-range",            // error 3
            Variables = new[]
            {
                new TemplateVariable("dup", "string"),
                new TemplateVariable("dup", "string")     // error 4
            },
            Files = new[]
            {
                new TemplateFile("", "ok.cs"),            // error 5
                new TemplateFile("ok.cs", "")              // error 6
            }
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.True(result.Errors.Count >= 4,
            $"Expected at least 4 errors but got {result.Errors.Count}: {string.Join("; ", result.Errors)}");
    }

    // ============================================================
    // ValidateAndThrow — Throws when invalid
    // ============================================================

    [Fact]
    public void ValidateAndThrow_Should_Throw_When_Invalid()
    {
        // Arrange
        var manifest = CreateValidManifest() with { Name = "" };

        // Act & Assert
        var ex = Assert.Throws<TemplateValidationException>(() => _validator.ValidateAndThrow(manifest));
        Assert.NotNull(ex.ValidationErrors);
        Assert.NotEmpty(ex.ValidationErrors);
    }

    // ============================================================
    // ValidateAndThrow — Does not throw when valid
    // ============================================================

    [Fact]
    public void ValidateAndThrow_Should_NotThrow_When_Valid()
    {
        // Arrange
        var manifest = CreateValidManifest();

        // Act & Assert (no exception)
        _validator.ValidateAndThrow(manifest);
    }

    // ============================================================
    // Validate — Null manifest throws
    // ============================================================

    [Fact]
    public void Validate_Should_Throw_When_ManifestIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _validator.Validate(null!));
    }

    // ============================================================
    // Validate — Supports SemVer with pre-release
    // ============================================================

    [Fact]
    public void Validate_Should_AcceptVersion_When_SemVerWithPreRelease()
    {
        // Arrange
        var manifest = CreateValidManifest() with
        {
            Version = "1.0.0-alpha.1+build.123"
        };

        // Act
        var result = _validator.Validate(manifest);

        // Assert
        Assert.True(result.IsValid, $"Expected valid but got errors: {string.Join("; ", result.Errors ?? Array.Empty<string>())}");
    }
}
