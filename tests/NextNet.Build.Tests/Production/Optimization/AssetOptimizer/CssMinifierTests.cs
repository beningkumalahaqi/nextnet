using NextNet.Build.Production.Optimization.AssetOptimizer;
using NextNet.IO;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.AssetOptimizer;

public class CssMinifierTests
{
    [Fact]
    public void Minify_Should_RemoveComments_When_Present()
    {
        var css = "/* This is a comment */ body { color: red; }";
        var result = CssMinifier.Minify(css);
        Assert.DoesNotContain("/*", result);
    }

    [Fact]
    public void Minify_Should_RemoveWhitespaceAroundStructuralChars_When_Present()
    {
        var css = "body { color : red ; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("color:red", result);
        Assert.DoesNotContain(" : ", result);
    }

    [Fact]
    public void Minify_Should_CollapseMultipleSpaces_When_Present()
    {
        var css = "body    {    color:    red;    }";
        var result = CssMinifier.Minify(css);
        Assert.True(result.Length < css.Length);
    }

    [Fact]
    public void Minify_Should_RemoveLastSemicolonBeforeClosingBrace_When_Present()
    {
        var css = "body { color: red; }";
        var result = CssMinifier.Minify(css);
        Assert.DoesNotContain(";}", result);
    }

    [Fact]
    public void Minify_Should_RemoveZeroUnits_When_Present()
    {
        var css = ".foo { margin: 0px; padding: 0em; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("0", result);
        Assert.DoesNotContain("0px", result);
        Assert.DoesNotContain("0em", result);
    }

    [Fact]
    public void Minify_Should_RemoveLeadingZeros_When_Present()
    {
        var css = ".foo { opacity: 0.5; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains(".5", result);
    }

    [Fact]
    public void Minify_Should_RemoveUrlQuotes_When_Present()
    {
        var css = ".foo { background: url('image.png'); }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("url(image.png)", result);
    }

    [Fact]
    public void Minify_Should_ShorthandHexColors_When_Possible()
    {
        var css = ".foo { color: #aabbcc; }";
        var result = CssMinifier.Minify(css);
        Assert.Contains("#abc", result);
    }

    [Fact]
    public void Minify_Should_ReturnInput_When_NullOrEmpty()
    {
        Assert.Null(CssMinifier.Minify(null!));
        Assert.Equal("", CssMinifier.Minify(""));
    }

    [Fact]
    public void CanHandle_Should_ReturnTrueForCss_When_Queried()
    {
        var optimizer = new CssMinifier(new DefaultSharpFileSystem());
        Assert.True(optimizer.CanHandle(".css"));
        Assert.False(optimizer.CanHandle(".js"));
        Assert.False(optimizer.CanHandle(".html"));
    }
}
