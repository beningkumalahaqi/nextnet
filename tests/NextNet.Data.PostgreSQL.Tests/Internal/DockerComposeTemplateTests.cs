using NextNet.Data.PostgreSQL.Internal;

namespace NextNet.Data.PostgreSQL.Tests.Internal;

/// <summary>
/// Tests for <see cref="DockerComposeTemplate"/> embedded resource handling.
/// </summary>
public sealed class DockerComposeTemplateTests
{
    [Fact]
    public void LoadTemplate_Should_ReturnNonEmptyString()
    {
        // Act
        var template = DockerComposeTemplate.LoadTemplate();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(template));
    }

    [Fact]
    public void LoadTemplate_Should_ContainPlaceholder()
    {
        // Act
        var template = DockerComposeTemplate.LoadTemplate();

        // Assert
        Assert.Contains("{{ProjectName}}", template);
    }

    [Fact]
    public void Render_Should_ReplaceProjectName()
    {
        // Act
        var rendered = DockerComposeTemplate.Render("MyApp");

        // Assert
        Assert.DoesNotContain("{{ProjectName}}", rendered);
        Assert.Contains("POSTGRES_DB: MyApp", rendered);
    }

    [Fact]
    public void Render_Should_ProduceValidContent()
    {
        // Act
        var rendered = DockerComposeTemplate.Render("TestProject");

        // Assert
        Assert.Contains("services:", rendered);
        Assert.Contains("postgres:", rendered);
        Assert.Contains("image: postgres:16-alpine", rendered);
        Assert.Contains("POSTGRES_DB: TestProject", rendered);
        Assert.Contains("volumes:", rendered);
    }

    [Fact]
    public void Render_Should_Throw_When_ProjectNameEmpty()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DockerComposeTemplate.Render(""));
    }

    [Fact]
    public void Render_Should_Throw_When_ProjectNameNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => DockerComposeTemplate.Render(null!));
    }
}
