using System.IO.Compression;

namespace NextNet.Build.Production.Compression.AlgorithmAdapters;

/// <summary>
/// Deflate compression adapter using System.IO.Compression.DeflateStream.
/// </summary>
public sealed class DeflateAdapter : ICompressionAdapter
{
    private readonly CompressionLevel _compressionLevel;

    /// <summary>
    /// Initializes a new instance of <see cref="DeflateAdapter"/>.
    /// </summary>
    /// <param name="compressionLevel">Compression level (1-9, where 9 is maximum). Mapped to CompressionLevel enum: 1-3=Fastest, 4-6=Optimal, 7-9=SmallestSize.</param>
    public DeflateAdapter(int compressionLevel = 5)
    {
        _compressionLevel = MapLevel(Math.Clamp(compressionLevel, 1, 9));
    }

    /// <inheritdoc />
    public string EncodingName => "deflate";

    /// <inheritdoc />
    public async Task CompressAsync(Stream source, Stream destination)
    {
        using var deflate = new DeflateStream(destination, _compressionLevel, leaveOpen: true);
        await source.CopyToAsync(deflate);
    }

    /// <inheritdoc />
    public async Task DecompressAsync(Stream source, Stream destination)
    {
        using var deflate = new DeflateStream(source, CompressionMode.Decompress, leaveOpen: true);
        await deflate.CopyToAsync(destination);
    }

    private static CompressionLevel MapLevel(int level) => level switch
    {
        <= 3 => CompressionLevel.Fastest,
        <= 6 => CompressionLevel.Optimal,
        _ => CompressionLevel.SmallestSize,
    };
}
