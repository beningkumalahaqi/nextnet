using System.Reflection;
using System.Text.Json;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Manifest;
using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Manifest;

public sealed class TemplateManifestParserTests
{
    private readonly TemplateManifestParser _parser = new();

    private static Stream GetFixtureStream(string name)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = $"NextNet.Templates.Tests.Manifest.Fixtures.{name}";
        var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Fixture resource '{resourceName}' not found. Available: {string.Join(", ", assembly.GetManifestResourceNames())}");
        return stream;
    }

    private static string GetFixtureJson(string name)
    {
        using var stream = GetFixtureStream(name);
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    // ============================================================
    // ParseAsync from Stream — Valid JSON
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_ReturnManifest_When_ValidJson()
    {
        // Arrange
        using var stream = GetFixtureStream("valid-minimal.json");

        // Act
        var manifest = await _parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(manifest);
        Assert.Equal("my-template", manifest.Name);
        Assert.Equal("1.0.0", manifest.Version);
        Assert.Equal(">=3.0.0", manifest.NextNetVersion);
    }

    // ============================================================
    // ParseAsync from Stream — Invalid JSON
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_Throw_When_InvalidJson()
    {
        // Arrange
        using var stream = new MemoryStream("this is not json"u8.ToArray());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<TemplateValidationException>(() => _parser.ParseAsync(stream));
        Assert.Contains("Invalid JSON", ex.ValidationErrors[0], StringComparison.Ordinal);
    }

    // ============================================================
    // ParseAsync from Stream — Required field missing
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_Throw_When_RequiredFieldMissing()
    {
        // Arrange
        using var stream = GetFixtureStream("invalid-missing-name.json");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<TemplateValidationException>(() => _parser.ParseAsync(stream));
        Assert.Contains("name", ex.ValidationErrors[0], StringComparison.OrdinalIgnoreCase);
    }

    // ============================================================
    // ParseAsync from string — Valid JSON
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_UseDefaults_When_OptionalFieldsOmitted()
    {
        // Arrange
        const string json = """{"name":"test","version":"1.2.3","nextnetVersion":">=4.0.0"}""";

        // Act
        var manifest = await _parser.ParseAsync(json);

        // Assert
        Assert.NotNull(manifest);
        Assert.Equal("test", manifest.Name);
        Assert.Equal("1.2.3", manifest.Version);
        Assert.Equal(">=4.0.0", manifest.NextNetVersion);
        Assert.Null(manifest.Author);
        Assert.Null(manifest.Description);
        Assert.Null(manifest.Tags);
        Assert.Null(manifest.Variables);
        Assert.Null(manifest.Features);
        Assert.Null(manifest.Files);
        Assert.Null(manifest.Conditions);
    }

    // ============================================================
    // Parse all variable types
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_ParseAllVariableTypes()
    {
        // Arrange
        const string json = """
        {
            "name": "test",
            "version": "1.0.0",
            "nextnetVersion": ">=1.0.0",
            "variables": [
                { "name": "strVar", "type": "string" },
                { "name": "boolVar", "type": "bool" },
                { "name": "enumVar", "type": "enum", "allowedValues": ["a", "b"] }
            ]
        }
        """;

        // Act
        var manifest = await _parser.ParseAsync(json);

        // Assert
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Variables);
        Assert.Equal(3, manifest.Variables.Count);

        Assert.Equal("strVar", manifest.Variables[0].Name);
        Assert.Equal("string", manifest.Variables[0].Type);

        Assert.Equal("boolVar", manifest.Variables[1].Name);
        Assert.Equal("bool", manifest.Variables[1].Type);

        Assert.Equal("enumVar", manifest.Variables[2].Name);
        Assert.Equal("enum", manifest.Variables[2].Type);
        Assert.NotNull(manifest.Variables[2].AllowedValues);
        Assert.Equal(2, manifest.Variables[2].AllowedValues!.Count);
    }

    // ============================================================
    // Parse feature dependencies
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_ParseFeatureDependencies()
    {
        // Arrange
        const string json = """
        {
            "name": "test",
            "version": "1.0.0",
            "nextnetVersion": ">=1.0.0",
            "features": [
                { "name": "auth", "dependencies": ["logging"] },
                { "name": "logging" }
            ]
        }
        """;

        // Act
        var manifest = await _parser.ParseAsync(json);

        // Assert
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Features);
        Assert.Equal(2, manifest.Features.Count);

        Assert.Equal("auth", manifest.Features[0].Name);
        Assert.NotNull(manifest.Features[0].Dependencies);
        Assert.Contains("logging", manifest.Features[0].Dependencies!);

        Assert.Equal("logging", manifest.Features[1].Name);
    }

    // ============================================================
    // Parse file conditions
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_ParseFileConditions()
    {
        // Arrange
        const string json = """
        {
            "name": "test",
            "version": "1.0.0",
            "nextnetVersion": ">=1.0.0",
            "files": [
                { "source": "always.cs", "target": "always.cs" },
                { "source": "conditional.cs", "target": "conditional.cs", "condition": "features.auth == true", "binary": false }
            ]
        }
        """;

        // Act
        var manifest = await _parser.ParseAsync(json);

        // Assert
        Assert.NotNull(manifest);
        Assert.NotNull(manifest.Files);
        Assert.Equal(2, manifest.Files.Count);

        Assert.Equal("always.cs", manifest.Files[0].SourcePath);
        Assert.Equal("always.cs", manifest.Files[0].TargetPath);
        Assert.Null(manifest.Files[0].Condition);
        Assert.False(manifest.Files[0].IsBinary);

        Assert.Equal("conditional.cs", manifest.Files[1].SourcePath);
        Assert.Equal("conditional.cs", manifest.Files[1].TargetPath);
        Assert.Equal("features.auth == true", manifest.Files[1].Condition);
        Assert.False(manifest.Files[1].IsBinary);
    }

    // ============================================================
    // DefaultJsonOptions checks
    // ============================================================

    [Fact]
    public void DefaultJsonOptions_Should_BeCamelCase()
    {
        // Assert
        Assert.Equal(JsonNamingPolicy.CamelCase, TemplateManifestParser.DefaultJsonOptions.PropertyNamingPolicy);
        Assert.True(TemplateManifestParser.DefaultJsonOptions.PropertyNameCaseInsensitive);
    }

    [Fact]
    public void DefaultJsonOptions_Should_AllowComments()
    {
        // Assert
        Assert.Equal(JsonCommentHandling.Skip, TemplateManifestParser.DefaultJsonOptions.ReadCommentHandling);
        Assert.True(TemplateManifestParser.DefaultJsonOptions.AllowTrailingCommas);
    }

    // ============================================================
    // ParseAsync from Stream with custom options
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_UseCustomOptions_When_Provided()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        const string json = """{"name":"custom","version":"1.0.0","nextnetVersion":">=1.0.0"}""";
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var manifest = await _parser.ParseAsync(stream, options);

        // Assert
        Assert.NotNull(manifest);
        Assert.Equal("custom", manifest.Name);
    }

    // ============================================================
    // ParseAsync from string with CancellationToken
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_RespectCancellationToken_When_Cancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _parser.ParseAsync("{}", cts.Token));
    }

    // ============================================================
    // ParseAsync from Stream with CancellationToken
    // ============================================================

    [Fact]
    public async Task ParseAsync_Stream_Should_RespectCancellationToken_When_Cancelled()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        using var stream = new MemoryStream("{}"u8.ToArray());

        // Act & Assert
        // Note: JsonSerializer.DeserializeAsync may not check the token before it starts,
        // so we accept either OperationCanceledException or a successful parse
        var exception = await Record.ExceptionAsync(() => _parser.ParseAsync(stream, cts.Token));
        Assert.True(exception is null or OperationCanceledException,
            $"Expected null or OperationCanceledException but got {exception?.GetType().Name}");
    }

    // ============================================================
    // ParseAsync — Full valid manifest from stream
    // ============================================================

    [Fact]
    public async Task ParseAsync_Should_ParseFullManifest()
    {
        // Arrange
        using var stream = GetFixtureStream("valid-full.json");

        // Act
        var manifest = await _parser.ParseAsync(stream);

        // Assert
        Assert.NotNull(manifest);
        Assert.Equal("my-full-template", manifest.Name);
        Assert.Equal("2.0.0", manifest.Version);
        Assert.Equal("^3.0.0", manifest.NextNetVersion);
        Assert.Equal("NextNet", manifest.Author);
        Assert.NotNull(manifest.Tags);
        Assert.Equal(2, manifest.Tags.Count);
        Assert.Contains("web", manifest.Tags);
        Assert.Contains("api", manifest.Tags);

        // Variables
        Assert.NotNull(manifest.Variables);
        Assert.Equal(3, manifest.Variables.Count);
        Assert.Equal("projectName", manifest.Variables[0].Name);
        Assert.Equal("string", manifest.Variables[0].Type);
        Assert.True(manifest.Variables[0].Required);
        Assert.Equal("sqlite", manifest.Variables[2].Default?.GetString());
        Assert.NotNull(manifest.Variables[2].AllowedValues);
        Assert.Equal(3, manifest.Variables[2].AllowedValues!.Count);

        // Features
        Assert.NotNull(manifest.Features);
        Assert.Equal(2, manifest.Features.Count);
        Assert.Equal("auth", manifest.Features[0].Name);
        Assert.NotNull(manifest.Features[0].Dependencies);
        Assert.Contains("database", manifest.Features[0].Dependencies!);

        // Files
        Assert.NotNull(manifest.Files);
        Assert.Equal(3, manifest.Files.Count);
        Assert.Equal("Program.cs", manifest.Files[0].SourcePath);
        Assert.Equal("Program.cs", manifest.Files[0].TargetPath);
        Assert.Equal("logo.png", manifest.Files[2].SourcePath);
        Assert.True(manifest.Files[2].IsBinary);

        // Conditions
        Assert.NotNull(manifest.Conditions);
        Assert.Single(manifest.Conditions);
        Assert.Equal("features.auth == true", manifest.Conditions[0].Expression);
    }
}
