using System.IO.Compression;
using System.Text;
using NextNet.Build.Optimization;
using Xunit;

namespace NextNet.Build.Tests.Optimization;

public class GzipCompressorTests
{
    [Fact]
    public void Compress_NullString_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GzipCompressor.Compress((string)null!));
    }

    [Fact]
    public void Compress_NullBytes_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => GzipCompressor.Compress((byte[])null!));
    }

    [Fact]
    public void Compress_EmptyString_ReturnsEmpty()
    {
        var result = GzipCompressor.Compress("");
        Assert.Empty(result);
    }

    [Fact]
    public void Compress_EmptyBytes_ReturnsEmpty()
    {
        var result = GzipCompressor.Compress(Array.Empty<byte>());
        Assert.Empty(result);
    }

    [Fact]
    public void Compress_String_ReturnsValidGzip()
    {
        var input = "<html><body>Hello, World!</body></html>";
        var compressed = GzipCompressor.Compress(input);

        // Verify it's valid gzip
        var decompressed = Decompress(compressed);
        Assert.Equal(input, decompressed);
    }

    [Fact]
    public void Compress_Bytes_ReturnsValidGzip()
    {
        var input = Encoding.UTF8.GetBytes("Hello, World!");
        var compressed = GzipCompressor.Compress(input);

        var decompressed = Decompress(compressed);
        Assert.Equal("Hello, World!", decompressed);
    }

    [Fact]
    public void Compress_ProducesSmallerOutput()
    {
        var input = new string('A', 10000); // Highly compressible
        var compressed = GzipCompressor.Compress(input);

        Assert.True(compressed.Length < Encoding.UTF8.GetByteCount(input),
            "Compressed size should be smaller than original");
    }

    [Fact]
    public void GetRatio_CalculatesCorrectly()
    {
        var ratio = GzipCompressor.GetRatio(1000, 300);
        Assert.Equal(30.0, ratio);
    }

    [Fact]
    public void GetRatio_WithZeroOriginal_Returns100()
    {
        var ratio = GzipCompressor.GetRatio(0, 0);
        Assert.Equal(100, ratio);
    }

    private static string Decompress(byte[] compressedData)
    {
        using var input = new MemoryStream(compressedData);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}
