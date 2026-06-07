using NextNet.Build.Production.Optimization.AssetOptimizer;
using Xunit;

namespace NextNet.Build.Tests.Production.Optimization.AssetOptimizer;

public class SvgOptimizerTests
{
    [Fact]
    public void Optimize_RemovesXmlDeclaration()
    {
        var svg = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><svg></svg>";
        var result = SvgOptimizer.Optimize(svg);
        Assert.DoesNotContain("<?xml", result);
    }

    [Fact]
    public void Optimize_RemovesComments()
    {
        var svg = "<svg><!-- comment --><circle/></svg>";
        var result = SvgOptimizer.Optimize(svg);
        Assert.DoesNotContain("<!--", result);
    }

    [Fact]
    public void Optimize_RemovesDoctype()
    {
        var svg = "<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\" \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\"><svg></svg>";
        var result = SvgOptimizer.Optimize(svg);
        Assert.DoesNotContain("DOCTYPE", result);
    }

    [Fact]
    public void Optimize_RemovesWhitespaceBetweenTags()
    {
        var svg = "<svg>\n  <circle/>\n</svg>";
        var result = SvgOptimizer.Optimize(svg);
        Assert.DoesNotContain(">  <", result);
    }

    [Fact]
    public void Optimize_RemovesUnnecessaryQuotesOnAttributes()
    {
        var svg = "<svg width=\"100\" height=\"200\"></svg>";
        var result = SvgOptimizer.Optimize(svg);
        // Should have width=100 without quotes
        Assert.Contains("width=100", result);
    }

    [Fact]
    public void Optimize_EmptyString_ReturnsEmpty()
    {
        Assert.Equal("", SvgOptimizer.Optimize(""));
    }

    [Fact]
    public void CanHandle_ReturnsTrueForSvg()
    {
        var optimizer = new SvgOptimizer(new NextNet.IO.DefaultSharpFileSystem());
        Assert.True(optimizer.CanHandle(".svg"));
        Assert.False(optimizer.CanHandle(".png"));
        Assert.False(optimizer.CanHandle(".css"));
    }
}
