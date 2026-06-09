using System.Security.Cryptography;
using System.Text;

namespace NextNet.Build.Production.Caching;

/// <summary>
/// Generates content hashes for cache validation and content-based filenames.
/// Uses SHA-256 for strong hashing with configurable output length.
/// </summary>
public sealed class ContentHashGenerator
{
    private readonly int _hashLength;

    /// <summary>
    /// Initializes a new instance of <see cref="ContentHashGenerator"/>.
    /// </summary>
    /// <param name="hashLength">Number of hex characters to use (default: 8, max: 64).</param>
    public ContentHashGenerator(int hashLength = 8)
    {
        _hashLength = Math.Clamp(hashLength, 4, 64);
    }

    /// <summary>
    /// Generates a content hash for the given byte array.
    /// </summary>
    public string GenerateHash(byte[] content)
    {
        if (content == null || content.Length == 0)
            return string.Empty;

        var hashBytes = SHA256.HashData(content);
        return Convert.ToHexString(hashBytes)[.._hashLength].ToLowerInvariant();
    }

    /// <summary>
    /// Generates a content hash for the given string.
    /// </summary>
    public string GenerateHash(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        return GenerateHash(Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Generates an ETag for the given byte array content.
    /// </summary>
    public string GenerateETag(byte[] content)
    {
        if (content == null || content.Length == 0)
            return string.Empty;

        var hash = SHA256.HashData(content);
        var hashStr = Convert.ToHexString(hash)[..16].ToLowerInvariant();
        return $"\"{hashStr}\"";
    }

    /// <summary>
    /// Generates an ETag for the given string content.
    /// </summary>
    public string GenerateETag(string content)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;

        return GenerateETag(Encoding.UTF8.GetBytes(content));
    }

    /// <summary>
    /// Creates a content-hashed filename: original.a1b2c3d4.ext
    /// </summary>
    /// <param name="fileName">Original filename (e.g., "styles.css").</param>
    /// <param name="content">The file content.</param>
    /// <returns>Hashed filename (e.g., "styles.a1b2c3d4.css").</returns>
    public string HashFileName(string fileName, byte[] content)
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));
        if (content == null)
            throw new ArgumentNullException(nameof(content));

        var extension = Path.GetExtension(fileName);
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var hash = GenerateHash(content);

        return string.IsNullOrEmpty(hash)
            ? fileName
            : $"{nameWithoutExt}.{hash}{extension}";
    }
}
