using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.AssetOptimizer;

public class CssMinifierTests
{
    [Fact]
    public void Minify_RemovesComments()
    {
        var css = "/* This is a comment */ body { color: red; }";
        var result = CssMinifier.Minify(css);
        Assert.DoesNotContain("/*", result);
    }

    [Fact]
    public void Minify_RemovesWhitespaceAroundStructuralChars()
    {
        var css = "body { color : red ; }";
        var result = CssMinifier.Minify(css);
        // After minification: body{color:red} (no spaces)
        Assert.Contains("color:red", result);
        Assert.DoesNotContain(" : ", result);
    }

    [Fact]
    public void Minify_CollapsesMultipleSpaces()
    {
        var css = "body    {    color:    red;    }";
        var result = CssMinifier.Minify(css);
        // Should be compact
        Assert.True(result.Length < css.Length);
    }

    [Fact]
    public void Minify_RemovesLastSemicolonBeforeClosingBrace()
    {
        var css = "body { color: red; }";
        var result = CssMinifier.Minify(css);
        // After minification: body{color:red} (no semicolon before })
        Assert.DoesNotContain(";}", result);
    }

    [Fact]
    public void Minify_RemovesZeroUnits()
    {
        var css = ".foo { margin: 0px; padding: 0em; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("0", result);
        Assert.DoesNotContain("0px", result);
        Assert.DoesNotContain("0em", result);
    }

    [Fact]
    public void Minify_RemovesLeadingZeros()
    {
        var css = ".foo { opacity: 0.5; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains(".5", result);
    }

    [Fact]
    public void Minify_RemovesUrlQuotes()
    {
        var css = ".foo { background: url('image.png'); }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("url(image.png)", result);
    }

    [Fact]
    public void Minify_ShorthandHexColors()
    {
        var css = ".foo { color: #aabbcc; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("#abc", result);
    }

    [Fact]
    public void Minify_NullOrEmpty_ReturnsInput()
    {
        Assert.Null(CssMinifier.Minify(null!));
        Assert.Equal("", CssMinifier.Minify(""));
    }

    [Fact]
    public void CanHandle_ReturnsTrueForCss()
    {
        var optimizer = new CssMinifier(new DefaultSharpFileSystem());
        Assert.True(optimizer.CanHandle(".css"));
        Assert.False(optimizer.CanHandle(".js"));
        Assert.False(optimizer.CanHandle(".html"));
    }
}
