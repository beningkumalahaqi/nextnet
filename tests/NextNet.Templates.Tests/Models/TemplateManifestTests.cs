using System.Text.Json;
using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplateManifestTests
{
    [Fact]
    public void Serialize_Should_RoundTrip_When_AllPropertiesSet()
    {
        // Arrange
        var manifest = new TemplateManifest(
            "test-template",
            "1.0.0",
            "3.0.0",
            "NextNet",
            "A test template",
            new[] { "test", "sample" },
            new[] { new TemplateVariable("name", "string") },
            new[] { new TemplateFeature("auth", "Authentication") },
            new[] { new TemplateFile("src/Program.cs", "Program.cs") },
            new[] { new TemplateCondition("features.auth") }
        );

        // Act
        var json = JsonSerializer.Serialize(manifest);
        var deserialized = JsonSerializer.Deserialize<TemplateManifest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal(manifest.Name, deserialized!.Name);
        Assert.Equal(manifest.Version, deserialized.Version);
        Assert.Equal(manifest.NextNetVersion, deserialized.NextNetVersion);
        Assert.Equal(manifest.Author, deserialized.Author);
        Assert.Equal(manifest.Description, deserialized.Description);
        Assert.Equal(manifest.Tags, deserialized.Tags);
        Assert.Equal(manifest.Variables!.Count, deserialized.Variables!.Count);
        Assert.Equal(manifest.Features!.Count, deserialized.Features!.Count);
        Assert.Equal(manifest.Files!.Count, deserialized.Files!.Count);
        Assert.Equal(manifest.Conditions!.Count, deserialized.Conditions!.Count);
    }

    [Fact]
    public void Serialize_Should_UseDefaults_When_PropertiesOmitted()
    {
        // Arrange
        var manifest = new TemplateManifest("minimal", "1.0.0", "3.0.0");

        // Act
        var json = JsonSerializer.Serialize(manifest);
        var deserialized = JsonSerializer.Deserialize<TemplateManifest>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Equal("minimal", deserialized!.Name);
        Assert.Equal("1.0.0", deserialized.Version);
        Assert.Equal("3.0.0", deserialized.NextNetVersion);
        Assert.Null(deserialized.Author);
        Assert.Null(deserialized.Description);
        Assert.Null(deserialized.Tags);
        Assert.Null(deserialized.Variables);
        Assert.Null(deserialized.Features);
        Assert.Null(deserialized.Files);
        Assert.Null(deserialized.Conditions);
    }

    [Fact]
    public void Constructor_Should_SetRequiredProperties()
    {
        // Arrange & Act
        var manifest = new TemplateManifest("my-template", "2.0.0", "3.5.0");

        // Assert
        Assert.Equal("my-template", manifest.Name);
        Assert.Equal("2.0.0", manifest.Version);
        Assert.Equal("3.5.0", manifest.NextNetVersion);
    }
}
