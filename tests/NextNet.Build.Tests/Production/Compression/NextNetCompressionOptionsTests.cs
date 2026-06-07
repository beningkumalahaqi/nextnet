using NextNet.Build.Production.Compression;
using Xunit;

namespace NextNet.Build.Tests.Production.Compression;

public class NextNetCompressionOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveReasonableDefaults()
    {
        var options = new NextNetCompressionOptions();
        Assert.True(options.EnableCompression);
        Assert.True(options.PreCompressAssets);
        Assert.Equal(5, options.CompressionLevel);
        Assert.Equal(256, options.MinimumResponseSize);
        Assert.Equal(CompressionAlgorithm.Brotli, options.PreferredAlgorithm);
        Assert.NotEmpty(options.MimeTypes);
    }

    [Fact]
    public void DefaultMimeTypes_IncludesCommonTypes()
    {
        var options = new NextNetCompressionOptions();
        Assert.Contains("text/html", options.MimeTypes);
        Assert.Contains("text/css", options.MimeTypes);
        Assert.Contains("application/javascript", options.MimeTypes);
        Assert.Contains("application/json", options.MimeTypes);
    }
}
