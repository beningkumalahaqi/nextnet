namespace NextNet.Build.Production.Compression.AlgorithmAdapters;

/// <summary>
/// Abstraction for individual compression algorithm implementations.
/// </summary>
public interface ICompressionAdapter
{
    /// <summary>
    /// Compresses data from the source stream into the destination stream.
    /// </summary>
    Task CompressAsync(Stream source, Stream destination);

    /// <summary>
    /// Decompresses data from the source stream into the destination stream.
    /// </summary>
    Task DecompressAsync(Stream source, Stream destination);

    /// <summary>
    /// Gets the content-encoding name (e.g., "br", "gzip", "zstd", "deflate").
    /// </summary>
    string EncodingName { get; }
}
