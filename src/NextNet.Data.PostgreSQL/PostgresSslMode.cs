namespace NextNet.Data.PostgreSQL;

/// <summary>
/// SSL modes for PostgreSQL connections, mapping to Npgsql's SSL support.
/// </summary>
/// <remarks>
/// <para>
/// These values map directly to PostgreSQL's <c>sslmode</c> connection parameter:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="Disable"/> — no encryption (development only)</description></item>
///   <item><description><see cref="Allow"/> — try SSL first, fall back to unencrypted</description></item>
///   <item><description><see cref="Prefer"/> — prefer SSL, allow fallback (default)</description></item>
///   <item><description><see cref="Require"/> — require SSL, fail if unavailable</description></item>
///   <item><description><see cref="VerifyCa"/> — require SSL and verify CA certificate</description></item>
///   <item><description><see cref="VerifyFull"/> — require SSL, verify CA, and verify hostname matches certificate</description></item>
/// </list>
/// <para>
/// For production, use <see cref="Require"/> or <see cref="VerifyFull"/>.
/// For local Docker development, use <see cref="Disable"/> or <see cref="Prefer"/>.
/// </para>
/// </remarks>
public enum PostgresSslMode
{
    /// <summary>No SSL encryption.</summary>
    Disable = 0,

    /// <summary>Try SSL first, fall back to unencrypted.</summary>
    Allow = 1,

    /// <summary>Prefer SSL, allow fallback (default).</summary>
    Prefer = 2,

    /// <summary>Require SSL, fail if unavailable.</summary>
    Require = 3,

    /// <summary>Require SSL and verify the server CA certificate.</summary>
    VerifyCa = 4,

    /// <summary>Require SSL, verify CA, and verify hostname matches certificate.</summary>
    VerifyFull = 5,
}
