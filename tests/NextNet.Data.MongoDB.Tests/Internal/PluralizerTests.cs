namespace NextNet.Data.MongoDB.Tests.Internal;

/// <summary>
/// Tests for <see cref="Pluralizer"/>.
/// </summary>
public sealed class PluralizerTests
{
    [Theory]
    [InlineData("User", "Users")]
    [InlineData("Category", "Categories")]
    [InlineData("Status", "Statuses")]
    [InlineData("Box", "Boxes")]
    [InlineData("Person", "People")]
    [InlineData("Child", "Children")]
    [InlineData("BlogPost", "BlogPosts")]
    [InlineData("UserRole", "UserRoles")]
    [InlineData("Entry", "Entries")]
    [InlineData("Query", "Queries")]
    [InlineData("Address", "Addresses")]
    [InlineData("Fox", "Foxes")]
    public void Pluralize_ShouldReturnCorrectPlural(string singular, string expectedPlural)
    {
        var result = Pluralizer.Pluralize(singular);
        Assert.Equal(expectedPlural, result);
    }

    [Fact]
    public void Pluralize_ShouldThrow_WhenNull()
    {
        Assert.Throws<ArgumentNullException>(() => Pluralizer.Pluralize(null!));
    }

    [Fact]
    public void Pluralize_ShouldThrow_WhenEmpty()
    {
        Assert.Throws<ArgumentException>(() => Pluralizer.Pluralize(string.Empty));
    }

    [Fact]
    public void Pluralize_ShouldThrow_WhenWhitespace()
    {
        Assert.Throws<ArgumentException>(() => Pluralizer.Pluralize("   "));
    }

    [Theory]
    [InlineData("Data", "Datas")] // Not irregular, falls through to default
    [InlineData("Index", "Indices")] // Irregular: index → indices
    public void Pluralize_ShouldHandleEdgeCases(string singular, string expectedPlural)
    {
        var result = Pluralizer.Pluralize(singular);
        Assert.Equal(expectedPlural, result);
    }

    [Fact]
    public void ToCamelCase_ShouldLowercaseFirstLetter()
    {
        var result = Pluralizer.ToCamelCase("BlogPosts");
        Assert.Equal("blogPosts", result);
    }

    [Fact]
    public void ToCamelCase_ShouldReturn_WhenAlreadyCamelCase()
    {
        var result = Pluralizer.ToCamelCase("blogPosts");
        Assert.Equal("blogPosts", result);
    }

    [Fact]
    public void ToCamelCase_ShouldReturnEmpty_WhenEmpty()
    {
        var result = Pluralizer.ToCamelCase(string.Empty);
        Assert.Equal(string.Empty, result);
    }
}
