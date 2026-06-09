using NextNet.Build.Optimization;
using Xunit;

namespace NextNet.Build.Tests.Optimization;

public class HtmlMinifierTests
{
    [Fact]
    public void Minify_Should_ThrowArgumentNullException_When_NullInput()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlMinifier.Minify(null!));
    }

    [Fact]
    public void Minify_Should_ReturnEmpty_When_EmptyString()
    {
        var result = HtmlMinifier.Minify("");
        Assert.Equal("", result);
    }

    [Fact]
    public void Minify_Should_RemoveHtmlComments_When_Present()
    {
        var input = "<div><!-- this is a comment --></div>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void Minify_Should_RemoveMultiLineComments_When_Present()
    {
        var input = "<div><!--\n  multi-line\n  comment\n--></div>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void Minify_Should_RemoveWhitespaceBetweenTags_When_Present()
    {
        var input = "<div>    \n  <span>text</span>  </div>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<div><span>text</span></div>", result);
    }

    [Fact]
    public void Minify_Should_ReduceMultipleSpaces_When_Present()
    {
        var input = "<p>hello     world</p>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<p>hello world</p>", result);
    }

    [Fact]
    public void Minify_Should_TrimDocument_When_LeadingTrailingWhitespace()
    {
        var input = "  \n  <html><body>Hello</body></html>  \n  ";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<html><body>Hello</body></html>", result);
    }

    [Fact]
    public void Minify_Should_RemoveWhitespaceBeforeSelfClose_When_Present()
    {
        var input = "<br />";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<br/>", result);
    }

    [Fact]
    public void Minify_Should_ProcessFullDocument_When_ComplexHtml()
    {
        var input = @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
    <title>  My Page  </title>
    <!-- styles -->
    <link rel=""stylesheet"" href=""/styles.css"">
</head>
<body>
    <div class=""container"">
        <h1>Hello World</h1>
        <p>This is a paragraph.</p>
    </div>
</body>
</html>";

        var result = HtmlMinifier.Minify(input);

        Assert.DoesNotContain("<!--", result);
        Assert.DoesNotContain("  ", result);
        Assert.False(result.StartsWith(' '));
        Assert.False(result.EndsWith(' '));
        Assert.Contains("Hello World", result);
        Assert.Contains("This is a paragraph.", result);
        Assert.True(result.Length < input.Length);
    }

    [Fact]
    public void GetRatio_Should_CalculateCorrectly_When_ValuesProvided()
    {
        var ratio = HtmlMinifier.GetRatio(1000, 500);
        Assert.Equal(50.0, ratio);
    }

    [Fact]
    public void GetRatio_Should_Return100_When_OriginalIsZero()
    {
        var ratio = HtmlMinifier.GetRatio(0, 0);
        Assert.Equal(100, ratio);
    }
}
