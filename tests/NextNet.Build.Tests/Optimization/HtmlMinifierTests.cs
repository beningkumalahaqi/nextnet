using NextNet.Build.Optimization;
using Xunit;

namespace NextNet.Build.Tests.Optimization;

public class HtmlMinifierTests
{
    [Fact]
    public void Minify_NullInput_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => HtmlMinifier.Minify(null!));
    }

    [Fact]
    public void Minify_EmptyString_ReturnsEmpty()
    {
        var result = HtmlMinifier.Minify("");
        Assert.Equal("", result);
    }

    [Fact]
    public void Minify_RemovesHtmlComments()
    {
        var input = "<div><!-- this is a comment --></div>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void Minify_RemovesMultiLineComments()
    {
        var input = "<div><!--\n  multi-line\n  comment\n--></div>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<div></div>", result);
    }

    [Fact]
    public void Minify_RemovesWhitespaceBetweenTags()
    {
        var input = "<div>    \n  <span>text</span>  </div>";
        var result = HtmlMinifier.Minify(input);
        // Whitespace between tags (>...<) should be removed
        Assert.Equal("<div><span>text</span></div>", result);
    }

    [Fact]
    public void Minify_ReducesMultipleSpaces()
    {
        var input = "<p>hello     world</p>";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<p>hello world</p>", result);
    }

    [Fact]
    public void Minify_TrimsDocument()
    {
        var input = "  \n  <html><body>Hello</body></html>  \n  ";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<html><body>Hello</body></html>", result);
    }

    [Fact]
    public void Minify_RemovesWhitespaceBeforeSelfClose()
    {
        var input = "<br />";
        var result = HtmlMinifier.Minify(input);
        Assert.Equal("<br/>", result);
    }

    [Fact]
    public void Minify_FullDocument()
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

        // Comments should be removed
        Assert.DoesNotContain("<!--", result);

        // Whitespace should be collapsed
        Assert.DoesNotContain("  ", result);

        // Result should be trimmed
        Assert.False(result.StartsWith(' '));
        Assert.False(result.EndsWith(' '));

        // Content should be preserved
        Assert.Contains("Hello World", result);
        Assert.Contains("This is a paragraph.", result);

        // Should be shorter than input
        Assert.True(result.Length < input.Length);
    }

    [Fact]
    public void GetRatio_CalculatesCorrectly()
    {
        var ratio = HtmlMinifier.GetRatio(1000, 500);
        Assert.Equal(50.0, ratio);
    }

    [Fact]
    public void GetRatio_WithZeroOriginal_Returns100()
    {
        var ratio = HtmlMinifier.GetRatio(0, 0);
        Assert.Equal(100, ratio);
    }
}
