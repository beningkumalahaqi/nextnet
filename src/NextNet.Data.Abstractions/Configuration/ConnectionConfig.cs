using System.Text.Json.Serialization;

namespace NextNet.Data.Abstractions.Configuration;

/// <summary>
/// Configuration for a single named database connection.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="ConnectionConfig"/> defines the connection string, provider type,
/// timeout, and whether the connection is enabled. These are stored in the
/// <see cref="DataConfig.Connections"/> dictionary keyed by logical name.
/// </para>
/// <example>
/// <code>
/// var config = new ConnectionConfig(
///     "Server=.;Database=MyApp;Trusted_Connection=true",
///     "EntityFramework",
///     30,
///     true);
/// </code>
/// </example>
/// </remarks>
/// <param name="ConnectionString">The connection string for the database. Must not be null or empty.</param>
/// <param name="Provider">The data provider name to use for this connection (e.g., "EntityFramework", "Dapper"). Defaults to <c>"EntityFramework"</c>.</param>
/// <param name="TimeoutSeconds">Optional command timeout in seconds. Must be between 1 and 3600. Defaults to 30.</param>
/// <param name="Enabled">Whether this connection is active. Defaults to <c>true</c>.</param>
/// <param name="PoolSize">Optional maximum pool size for this connection. Provider-dependent. Defaults to <c>null</c> (provider default).</param>
/// <param name="Tags">Optional tags for categorizing connections (e.g., "readonly", "legacy", "tenant:acme"). Defaults to <c>null</c>.</param>
public sealed record ConnectionConfig(
    [property: JsonPropertyName("connectionString")]
    string ConnectionString,

    [property: JsonPropertyName("provider")]
    string Provider = "EntityFramework",

    [property: JsonPropertyName("timeoutSeconds")]
    int TimeoutSeconds = 30,

    [property: JsonPropertyName("enabled")]
    bool Enabled = true,

    [property: JsonPropertyName("poolSize")]
    int? PoolSize = null,

    [property: JsonPropertyName("tags")]
    IReadOnlyList<string>? Tags = null
)
{
    /// <summary>
    /// Gets or sets the logical connection name (e.g., "Primary", "Analytics").
    /// This is set at registration time by the multi-database selector and
    /// used for keying connections in the connection registries.
    /// </summary>
    [JsonIgnore]
    public string? Name { get; init; }
}
