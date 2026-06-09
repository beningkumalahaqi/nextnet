using System.Text;
using NextNet.Build.Production.Compression.AlgorithmAdapters;
using Xunit;

namespace NextNet.Build.Tests.Production.Compression;

public class CompressionAdapterTests
{
    [Fact]
    public async Task BrotliAdapter_Should_RoundTrip_When_CompressAndDecompress()
    {
        var adapter = new BrotliAdapter();
        var input = string.Join(" ", Enumerable.Repeat("Hello World Compression Test", 50));
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var inputStream = new MemoryStream(inputBytes);
        using var compressedStream = new MemoryStream();
        await adapter.CompressAsync(inputStream, compressedStream);

        compressedStream.Seek(0, SeekOrigin.Begin);
        using var decompressedStream = new MemoryStream();
        await adapter.DecompressAsync(compressedStream, decompressedStream);

        var output = Encoding.UTF8.GetString(decompressedStream.ToArray());
        Assert.Equal(input, output);
    }

    [Fact]
    public async Task GzipAdapter_Should_RoundTrip_When_CompressAndDecompress()
    {
        var adapter = new GzipAdapter();
        var input = string.Join(" ", Enumerable.Repeat("GZip Compression Test", 50));
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var inputStream = new MemoryStream(inputBytes);
        using var compressedStream = new MemoryStream();
        await adapter.CompressAsync(inputStream, compressedStream);

        compressedStream.Seek(0, SeekOrigin.Begin);
        using var decompressedStream = new MemoryStream();
        await adapter.DecompressAsync(compressedStream, decompressedStream);

        var output = Encoding.UTF8.GetString(decompressedStream.ToArray());
        Assert.Equal(input, output);
    }

    [Fact]
    public async Task DeflateAdapter_Should_RoundTrip_When_CompressAndDecompress()
    {
        var adapter = new DeflateAdapter();
        var input = string.Join(" ", Enumerable.Repeat("Deflate Compression Test", 50));
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var inputStream = new MemoryStream(inputBytes);
        using var compressedStream = new MemoryStream();
        await adapter.CompressAsync(inputStream, compressedStream);

        compressedStream.Seek(0, SeekOrigin.Begin);
        using var decompressedStream = new MemoryStream();
        await adapter.DecompressAsync(compressedStream, decompressedStream);

        var output = Encoding.UTF8.GetString(decompressedStream.ToArray());
        Assert.Equal(input, output);
    }

    [Fact]
    public async Task ZstdAdapter_Should_FallbackGracefully_When_CompressAndDecompress()
    {
        var adapter = new ZstdAdapter();
        var input = "Hello, World! Zstd compression test (falls back to Brotli).";
        var inputBytes = Encoding.UTF8.GetBytes(input);

        using var inputStream = new MemoryStream(inputBytes);
        using var compressedStream = new MemoryStream();
        await adapter.CompressAsync(inputStream, compressedStream);

        Assert.True(compressedStream.Length < inputBytes.Length);

        compressedStream.Seek(0, SeekOrigin.Begin);
        using var decompressedStream = new MemoryStream();
        await adapter.DecompressAsync(compressedStream, decompressedStream);

        var output = Encoding.UTF8.GetString(decompressedStream.ToArray());
        Assert.Equal(input, output);
    }

    [Fact]
    public void Adapters_Should_ReturnCorrectEncodingNames_When_Queried()
    {
        Assert.Equal("br", new BrotliAdapter().EncodingName);
        Assert.Equal("gzip", new GzipAdapter().EncodingName);
        Assert.Equal("deflate", new DeflateAdapter().EncodingName);
        Assert.Equal("zstd", new ZstdAdapter().EncodingName);
    }
}
