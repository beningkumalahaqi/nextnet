using NextNet.Core.Extensions;
using Xunit;

namespace NextNet.Core.Tests.Extensions;

public class StringCaseHelperTests
{
    [Theory]
    [InlineData("my-blog", "MyBlog")]
    [InlineData("my-blog-app", "MyBlogApp")]
    [InlineData("my_app", "MyApp")]
    [InlineData("MyBlog", "MyBlog")]
    [InlineData("a", "A")]
    [InlineData("", "App")]
    [InlineData("my--blog", "MyBlog")]
    [InlineData("my_blog_test", "MyBlogTest")]
    [InlineData("ALLCAPS", "ALLCAPS")]
    [InlineData("123blog", "123blog")]
    [InlineData("my_app-blog", "MyAppBlog")]
    public void ToPascalCase_Should_ConvertCorrectly(string input, string expected)
    {
        var result = StringCaseHelper.ToPascalCase(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToPascalCase_Should_ReturnApp_When_Null()
    {
        var result = StringCaseHelper.ToPascalCase(null);
        Assert.Equal("App", result);
    }
}
