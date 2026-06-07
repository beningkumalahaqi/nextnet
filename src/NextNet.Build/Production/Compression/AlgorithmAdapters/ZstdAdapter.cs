using System.IO.Compression;

namespace NextNet.Build.Production.Compression.AlgorithmAdapters;

/// <summary>
/// Zstandard compression adapter.
/// Note: .NET does not include Zstd natively. This is a bridge implementation.
/// For production use, install a NuGet package like "ZstdSharp" or "K4os.Compression.LZ4".
/// This implementation falls back to Brotli-compatible behavior as a placeholder.
/// </summary>
public class ZstdAdapter : ICompressionAdapter
{
    private readonly int _compressionLevel;
    private readonly Lazy<ICompressionAdapter> _fallback;

    /// <summary>
    /// Initializes a new instance of <see cref="ZstdAdapter"/>.
    /// </summary>
    public ZstdAdapter(int compressionLevel = 5)
    {
        _compressionLevel = Math.Clamp(compressionLevel, 1, 9);
        _fallback = new Lazy<ICompressionAdapter>(() => new BrotliAdapter(_compressionLevel));
    }

    /// <inheritdoc />
    public string EncodingName => "zstd";

    /// <inheritdoc />
    public async Task CompressAsync(Stream source, Stream destination)
    {
        // Fallback to Brotli until a Zstd package is added
        if (TryGetZstdStream(source, destination, out var zstdStream) && zstdStream != null)
        {
            await source.CopyToAsync(zstdStream);
        }
        else
        {
            await _fallback.Value.CompressAsync(source, destination);
        }
    }

    /// <inheritdoc />
    public async Task DecompressAsync(Stream source, Stream destination)
    {
        // Fallback to Brotli
        if (TryGetZstdDecompressStream(source, destination, out var zstdStream) && zstdStream != null)
        {
            await zstdStream.CopyToAsync(destination);
        }
        else
        {
            await _fallback.Value.DecompressAsync(source, destination);
        }
    }

    private static bool TryGetZstdStream(Stream source, Stream destination, out Stream? zstdStream)
    {
        // Attempt to use ZstdSharp if available via reflection
        try
        {
            var assembly = System.Reflection.Assembly.Load("ZstdSharp");
            var type = assembly.GetType("ZstdSharp.CompressionStream");
            if (type != null)
            {
                zstdStream = (Stream?)Activator.CreateInstance(type, destination, 3);
                return zstdStream != null;
            }
        }
        catch
        {
            // ZstdSharp not available
        }

        zstdStream = null;
        return false;
    }

    private static bool TryGetZstdDecompressStream(Stream source, Stream destination, out Stream? zstdStream)
    {
        try
        {
            var assembly = System.Reflection.Assembly.Load("ZstdSharp");
            var type = assembly.GetType("ZstdSharp.DecompressionStream");
            if (type != null)
            {
                zstdStream = (Stream?)Activator.CreateInstance(type, source);
                return zstdStream != null;
            }
        }
        catch
        {
            // ZstdSharp not available
        }

        zstdStream = null;
        return false;
    }
}
