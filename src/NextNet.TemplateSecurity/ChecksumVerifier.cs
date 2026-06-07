namespace NextNet.TemplateSecurity;

using System.Security.Cryptography;

/// <summary>
/// Computes and verifies SHA-256 checksums for template packages.
/// </summary>
public sealed class ChecksumVerifier
{
    /// <summary>
    /// Computes the SHA-256 hash of a stream and returns it as a lowercase hex string.
    /// </summary>
    /// <param name="stream">The stream to hash.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A 64-character lowercase hex string representing the SHA-256 hash.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    public async Task<string> ComputeChecksumAsync(Stream stream, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        using var sha = SHA256.Create();
        var hash = await sha.ComputeHashAsync(stream, ct);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>
    /// Streamingly verifies a SHA-256 checksum against a stream's content.
    /// </summary>
    /// <param name="stream">The stream to verify.</param>
    /// <param name="expectedChecksum">The expected checksum value (case-insensitive).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if the checksum matches; otherwise <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="expectedChecksum"/> is null or whitespace.</exception>
    public async Task<bool> VerifyAsync(Stream stream, string expectedChecksum, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(expectedChecksum);

        var actual = await ComputeChecksumAsync(stream, ct);
        return string.Equals(actual, expectedChecksum, StringComparison.OrdinalIgnoreCase);
    }
}
