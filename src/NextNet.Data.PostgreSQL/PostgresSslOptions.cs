namespace NextNet.Data.PostgreSQL;

/// <summary>
/// SSL/TLS configuration for PostgreSQL connections.
/// Maps to Npgsql SSL connection string parameters.
/// </summary>
/// <remarks>
/// <para>
/// SSL modes align with PostgreSQL's <c>sslmode</c> connection parameter:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="PostgresSslMode.Disable"/> — no encryption (development only)</description></item>
///   <item><description><see cref="PostgresSslMode.Allow"/> — try SSL first, fall back to unencrypted</description></item>
///   <item><description><see cref="PostgresSslMode.Prefer"/> — prefer SSL, allow fallback (default)</description></item>
///   <item><description><see cref="PostgresSslMode.Require"/> — require SSL, fail if unavailable</description></item>
///   <item><description><see cref="PostgresSslMode.VerifyCa"/> — require SSL and verify CA certificate</description></item>
///   <item><description><see cref="PostgresSslMode.VerifyFull"/> — require SSL, verify CA, and verify hostname</description></item>
/// </list>
/// <para>
/// For production, use <see cref="PostgresSslMode.Require"/> or <see cref="PostgresSslMode.VerifyFull"/>.
/// For local Docker development, use <see cref="PostgresSslMode.Disable"/> or <see cref="PostgresSslMode.Prefer"/>.
/// </para>
/// </remarks>
public sealed class PostgresSslOptions
{
    /// <summary>
    /// SSL mode. Defaults to <see cref="PostgresSslMode.Prefer"/>.
    /// </summary>
    public PostgresSslMode Mode { get; set; } = PostgresSslMode.Prefer;

    /// <summary>
    /// Path to the client SSL certificate file (.pfx or .p12) for mutual TLS.
    /// Only needed when the server requires client certificates.
    /// </summary>
    public string? ClientCertificatePath { get; set; }

    /// <summary>
    /// Password for the client certificate file, if encrypted.
    /// </summary>
    public string? ClientCertificatePassword { get; set; }

    /// <summary>
    /// Path to the root CA certificate file for certificate validation.
    /// Only used when <see cref="Mode"/> is <see cref="PostgresSslMode.VerifyCa"/>
    /// or <see cref="PostgresSslMode.VerifyFull"/>.
    /// </summary>
    public string? RootCertificatePath { get; set; }

    /// <summary>
    /// Whether to trust the server certificate without validation.
    /// Equivalent to setting <c>Trust Server Certificate=true</c> in the connection string.
    /// Defaults to <c>false</c>.
    /// Only use in development or when using self-signed certificates.
    /// </summary>
    public bool TrustServerCertificate { get; set; } = false;
}
