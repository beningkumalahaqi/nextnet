using System.Text.Json;
using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplateVariableTests
{
    [Fact]
    public void Constructor_Should_SetAllProperties()
    {
        // Arrange
        var defaultValue = JsonSerializer.Deserialize<JsonElement>("\"default-value\"");

        // Act
        var variable = new TemplateVariable(
            "projectName",
            "string",
            defaultValue,
            "The project name",
            true,
            new[] { "App1", "App2" }
        );

        // Assert
        Assert.Equal("projectName", variable.Name);
        Assert.Equal("string", variable.Type);
        Assert.NotNull(variable.Default);
        Assert.Equal("default-value", variable.Default!.Value.GetString());
        Assert.Equal("The project name", variable.Description);
        Assert.True(variable.Required);
        Assert.Equal(new[] { "App1", "App2" }, variable.AllowedValues);
    }

    [Fact]
    public void Type_Should_DefaultToString()
    {
        // Arrange & Act
        var variable = new TemplateVariable("name");

        // Assert
        Assert.Equal("string", variable.Type);
    }
}
