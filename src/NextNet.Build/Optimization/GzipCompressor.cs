using System.IO.Compression;
using System.Text;

namespace NextNet.Build.Optimization;

/// <summary>
/// Provides GZip compression for generated HTML files during static site generation.
/// Produces <c>.gz</c> files suitable for pre-compressed serving by web servers.
/// </summary>
public static class GzipCompressor
{
    /// <summary>
    /// Compresses the given HTML string to a gzipped byte array.
    /// </summary>
    /// <param name="html">The HTML content to compress.</param>
    /// <returns>A gzip-compressed byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="html"/> is null.</exception>
    public static byte[] Compress(string html)
    {
        if (html == null) throw new ArgumentNullException(nameof(html));

        var bytes = Encoding.UTF8.GetBytes(html);
        return Compress(bytes);
    }

    /// <summary>
    /// Compresses the given byte array using GZip.
    /// </summary>
    /// <param name="data">The data to compress.</param>
    /// <returns>A gzip-compressed byte array.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="data"/> is null.</exception>
    public static byte[] Compress(byte[] data)
    {
        if (data == null) throw new ArgumentNullException(nameof(data));
        if (data.Length == 0) return Array.Empty<byte>();

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: false))
        {
            gzip.Write(data, 0, data.Length);
        }

        return output.ToArray();
    }

    /// <summary>
    /// Gets the compression ratio as a percentage (100 = no compression, 50 = half size).
    /// </summary>
    /// <param name="originalSize">The original size in bytes.</param>
    /// <param name="compressedSize">The compressed size in bytes.</param>
    /// <returns>The ratio (compressed / original * 100).</returns>
    public static double GetRatio(int originalSize, int compressedSize)
    {
        if (originalSize <= 0) return 100;
        return Math.Round((double)compressedSize / originalSize * 100, 1);
    }
}
