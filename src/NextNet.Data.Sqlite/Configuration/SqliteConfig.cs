using System.Text.Json.Serialization;

namespace NextNet.Data.Sqlite.Configuration;

/// <summary>
/// SQLite-specific configuration section in <c>nextnet.config.json</c>.
/// Mapped from <c>"data.connections.default"</c> when the provider is <c>"sqlite"</c>.
/// </summary>
/// <remarks>
/// <para>
/// This model is consumed by both the runtime (for reading config at startup)
/// and the CLI (for reading/writing <c>nextnet.config.json</c> during
/// <c>nextnet db init sqlite</c>).
/// </para>
/// </remarks>
public sealed class SqliteConfig
{
    /// <summary>
    /// Path to the SQLite database file, relative to the project root.
    /// Defaults to <c>"{project-name}.db"</c>.
    /// </summary>
    [JsonPropertyName("dataSource")]
    public string? DataSource { get; set; }

    /// <summary>
    /// Whether to use an in-memory database.
    /// </summary>
    [JsonPropertyName("inMemory")]
    public bool InMemory { get; set; }

    /// <summary>
    /// SQLite cache mode: <c>"Default"</c>, <c>"Private"</c>, <c>"Shared"</c>.
    /// </summary>
    [JsonPropertyName("cache")]
    public string? Cache { get; set; }
}
