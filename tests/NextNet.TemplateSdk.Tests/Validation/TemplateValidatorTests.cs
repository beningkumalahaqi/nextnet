using NextNet.Templates.Models;
using NextNet.TemplateSdk.Validation;
using NextNet.TemplateSdk.Validation.Rules;
using Xunit;

namespace NextNet.TemplateSdk.Tests.Validation;

public class TemplateValidatorTests
{
    // ============================================================
    // Helper methods
    // ============================================================

    private static TemplateManifest CreateValidManifest()
    {
        return new TemplateManifest(
            Name: "test-template",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Author: "TestAuthor",
            Description: "A test template",
            Tags: new[] { "test", "template" },
            Variables: new[]
            {
                new TemplateVariable("projectName", "string", Default: System.Text.Json.JsonSerializer.SerializeToElement("MyProject"), Required: true),
                new TemplateVariable("framework", "enum", AllowedValues: new[] { "net8.0", "net9.0" })
            }
        );
    }

    private static Dictionary<string, byte[]> CreateFileDictionary()
    {
        return new Dictionary<string, byte[]>
        {
            ["src/Program.cs"] = System.Text.Encoding.UTF8.GetBytes("namespace {{projectName}};"),
            ["src/Startup.cs"] = System.Text.Encoding.UTF8.GetBytes("class Startup { }")
        };
    }

    // ============================================================
    // Test: Valid manifest returns valid result
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnValid_When_ManifestIsValid()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = CreateValidManifest();

        // Act
        var result = await validator.ValidateAsync(manifest);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Errors);
        Assert.Null(result.Warnings);
    }

    // ============================================================
    // Test: Missing name returns error
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnError_When_ManifestMissingName()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0"
        );

        // Act
        var result = await validator.ValidateAsync(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("name"));
    }

    // ============================================================
    // Test: Missing file referenced in manifest returns error
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnError_When_FileMissing()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new[]
            {
                new TemplateFile("missing-file.cs", "missing-file.cs")
            }
        );

        var package = new TemplatePackage(manifest, new Dictionary<string, byte[]>());

        // Act
        var result = await validator.ValidateAsync(package);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("missing-file.cs"));
    }

    // ============================================================
    // Test: Enum variable without allowed values returns error
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnError_When_EnumVariableHasNoAllowedValues()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Variables: new[]
            {
                new TemplateVariable("framework", "enum")
            }
        );

        // Act
        var result = await validator.ValidateAsync(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("framework") && e.Contains("AllowedValues"));
    }

    // ============================================================
    // Test: Placeholder without matching variable returns warning
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnWarning_When_PlaceholderHasNoVariable()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Variables: new[]
            {
                new TemplateVariable("projectName", "string")
            },
            Files: new[]
            {
                new TemplateFile("src/Program.cs", "Program.cs"),
                new TemplateFile("src/Config.cs", "Config.cs")
            }
        );

        var files = new Dictionary<string, byte[]>
        {
            ["src/Program.cs"] = System.Text.Encoding.UTF8.GetBytes("namespace {{projectName}};"),
            ["src/Config.cs"] = System.Text.Encoding.UTF8.GetBytes("var db = \"{{undefinedVar}}\";")
        };

        var package = new TemplatePackage(manifest, files);

        // Act
        var result = await validator.ValidateAsync(package);

        // Assert
        Assert.True(result.IsValid); // No errors
        Assert.NotNull(result.Warnings);
        Assert.Contains(result.Warnings, w => w.Contains("undefinedVar"));
    }

    // ============================================================
    // Test: Invalid condition syntax returns error
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnError_When_ConditionIsInvalid()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new[]
            {
                new TemplateFile("src/Feature.cs", "Feature.cs", Condition: "features.auth =="), // trailing operator
                new TemplateFile("src/Normal.cs", "Normal.cs", Condition: "features.logging == true")
            }
        );

        var files = new Dictionary<string, byte[]>
        {
            ["src/Feature.cs"] = Array.Empty<byte>(),
            ["src/Normal.cs"] = Array.Empty<byte>()
        };

        var package = new TemplatePackage(manifest, files);

        // Act
        var result = await validator.ValidateAsync(package);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("features.auth"));
    }

    // ============================================================
    // Test: Executable file in package returns error
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnError_When_ExecutableFilePresent()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: new[]
            {
                new TemplateFile("src/tool.exe", "src/tool.exe"),
                new TemplateFile("src/script.sh", "src/script.sh")
            }
        );

        var files = new Dictionary<string, byte[]>
        {
            ["src/tool.exe"] = Array.Empty<byte>(),
            ["src/script.sh"] = Array.Empty<byte>()
        };

        var package = new TemplatePackage(manifest, files);

        // Act
        var result = await validator.ValidateAsync(package);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("tool.exe"));
        Assert.Contains(result.Errors, e => e.Contains("script.sh"));
    }

    // ============================================================
    // Test: No issues at all
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_ReturnValid_When_NoIssues()
    {
        // Arrange
        var validator = new TemplateValidator();
        var manifest = CreateValidManifest();
        var files = CreateFileDictionary();
        var package = new TemplatePackage(manifest, files);

        // Act
        var result = await validator.ValidateAsync(package);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.Errors);
    }

    // ============================================================
    // Test: Cancellation handling
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_HandleCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var validator = new TemplateValidator();
        var manifest = new TemplateManifest(
            Name: "test",
            Version: "1.0.0",
            NextNetVersion: ">=3.0.0",
            Files: Enumerable.Range(0, 1000)
                .Select(i => new TemplateFile($"file{i}.txt", $"file{i}.txt"))
                .ToArray()
        );

        var files = Enumerable.Range(0, 1000)
            .ToDictionary(i => $"file{i}.txt", i => Array.Empty<byte>());
        var package = new TemplatePackage(manifest, files);

        // Act — cancel before validation completes
        cts.Cancel();

        // Assert — TaskCanceledException is thrown because Task.Run wraps the OperationCanceledException
        await Assert.ThrowsAsync<TaskCanceledException>(() =>
            validator.ValidateAsync(package, cts.Token));
    }

    // ============================================================
    // Test: Custom rules
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_SupportCustomRules()
    {
        // Arrange
        var customRule = new TestCustomRule();
        var validator = new TemplateValidator(new[] { customRule });
        var manifest = CreateValidManifest();

        // Act
        var result = await validator.ValidateAsync(manifest);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotNull(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("custom-rule-check"));
    }

    /// <summary>
    /// A custom rule used for testing rule extensibility.
    /// </summary>
    private sealed class TestCustomRule : ValidationRule
    {
        public override string Name => "test-custom-rule";
        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public override IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            yield return new ValidationResult
            {
                RuleName = Name,
                Severity = ValidationSeverity.Error,
                Message = "custom-rule-check triggered",
                Suggestion = "This is a test suggestion."
            };
        }
    }

    // ============================================================
    // Test: Rule exceptions are caught gracefully
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateAsync_Should_CatchRuleExceptions()
    {
        // Arrange
        var brokenRule = new BrokenRule();
        var validator = new TemplateValidator(new[] { brokenRule });
        var manifest = CreateValidManifest();

        // Act
        var result = await validator.ValidateAsync(manifest);

        // Assert
        Assert.True(result.IsValid); // No errors, just a warning for the broken rule
        Assert.NotNull(result.Warnings);
        Assert.Contains(result.Warnings, w => w.Contains("broken-rule"));
    }

    /// <summary>
    /// A rule that throws an exception to test error handling.
    /// </summary>
    private sealed class BrokenRule : ValidationRule
    {
        public override string Name => "broken-rule";
        public override ValidationSeverity DefaultSeverity => ValidationSeverity.Error;

        public override IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            throw new InvalidOperationException("Something went wrong in the rule.");
        }
    }

    // ============================================================
    // Test: SuggestionEngine provides appropriate suggestions
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public void SuggestionEngine_Should_ProvideContextualSuggestions()
    {
        // Arrange
        var engine = new NextNet.TemplateSdk.Validation.Suggestions.SuggestionEngine();

        var manifestNameResult = new ValidationResult
        {
            RuleName = "manifestSchema",
            Severity = ValidationSeverity.Error,
            Message = "Manifest 'name' is required."
        };

        var placeholderResult = new ValidationResult
        {
            RuleName = "placeholderCoverage",
            Severity = ValidationSeverity.Warning,
            Message = "Placeholder '{foo}' references undefined variable 'foo'."
        };

        // Act
        var nameSuggestion = engine.GetSuggestion(manifestNameResult);
        var placeholderSuggestion = engine.GetSuggestion(placeholderResult);

        // Assert
        Assert.NotNull(nameSuggestion);
        Assert.Contains("name", nameSuggestion, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(placeholderSuggestion);
        Assert.Contains("foo", placeholderSuggestion);
    }

    // ============================================================
    // Test: ValidationReport aggregates correctly
    // ============================================================

    [Fact]
    [Trait("Category", "Unit")]
    public void ValidationReport_Should_AggregateCountsCorrectly()
    {
        // Arrange
        var results = new[]
        {
            new ValidationResult { RuleName = "r1", Severity = ValidationSeverity.Error, Message = "Error 1" },
            new ValidationResult { RuleName = "r2", Severity = ValidationSeverity.Error, Message = "Error 2" },
            new ValidationResult { RuleName = "r3", Severity = ValidationSeverity.Warning, Message = "Warning 1" },
            new ValidationResult { RuleName = "r4", Severity = ValidationSeverity.Info, Message = "Info 1" }
        };

        var report = new ValidationReport { Results = results };

        // Assert
        Assert.Equal(2, report.ErrorCount);
        Assert.Equal(1, report.WarningCount);
        Assert.False(report.IsValid);
    }
}
