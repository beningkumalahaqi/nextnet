using NextNet.Templates.Models;
using Xunit;

namespace NextNet.Templates.Tests.Models;

public class TemplateConditionTests
{
    [Fact]
    public void Constructor_Should_SetExpression()
    {
        // Arrange & Act
        var condition = new TemplateCondition("features.auth == true");

        // Assert
        Assert.Equal("features.auth == true", condition.Expression);
    }

    [Fact]
    public void Type_Should_DefaultToExpression()
    {
        // Arrange & Act
        var condition = new TemplateCondition("name != null");

        // Assert
        Assert.Equal("expression", condition.Type);
    }
}
