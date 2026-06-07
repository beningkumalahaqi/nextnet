using System.Text.Json.Serialization;

namespace NextNet.Data.PostgreSQL.Configuration;

/// <summary>
/// PostgreSQL-specific configuration section in <c>nextnet.config.json</c>.
/// Mapped from <c>"data.connections.default"</c> when the provider is <c>"postgresql"</c> or <c>"postgres"</c>.
/// </summary>
/// <remarks>
/// <para>
/// This model is consumed by both the runtime (for reading config at startup)
/// and the CLI (for reading/writing <c>nextnet.config.json</c> during
/// <c>nextnet db init postgres</c>).
/// </para>
/// </remarks>
public sealed class PostgresConfig
{
    /// <summary>
    /// Server hostname. Defaults to <c>"localhost"</c>.
    /// </summary>
    [JsonPropertyName("host")]
    public string? Host { get; set; }

    /// <summary>
    /// Server port. Defaults to <c>5432</c>.
    /// </summary>
    [JsonPropertyName("port")]
    public int? Port { get; set; }

    /// <summary>
    /// Database name.
    /// </summary>
    [JsonPropertyName("database")]
    public string? Database { get; set; }

    /// <summary>
    /// Connection username.
    /// </summary>
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Connection password (stored as plaintext — use environment variables in production).
    /// </summary>
    [JsonPropertyName("password")]
    public string? Password { get; set; }

    /// <summary>
    /// SSL mode string: <c>"Disable"</c>, <c>"Allow"</c>, <c>"Prefer"</c>, <c>"Require"</c>,
    /// <c>"VerifyCa"</c>, <c>"VerifyFull"</c>.
    /// </summary>
    [JsonPropertyName("sslMode")]
    public string? SslMode { get; set; }

    /// <summary>
    /// Whether to use a local Docker PostgreSQL instance.
    /// </summary>
    [JsonPropertyName("useDocker")]
    public bool UseDocker { get; set; }

    /// <summary>
    /// Docker container name (defaults to <c>"nextnet-postgres"</c>).
    /// </summary>
    [JsonPropertyName("containerName")]
    public string? ContainerName { get; set; }
}
