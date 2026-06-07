using System.Text.Json.Serialization;

namespace NextNet.Data.Abstractions.Configuration;

/// <summary>
/// Root configuration for the NextNet data layer.
/// Maps to the <c>"data"</c> section in <c>nextnet.config.json</c>.
/// </summary>
/// <remarks>
/// <para>
/// This record defines all data-layer configuration options, including named
/// database connections, migration settings, and scaffolding/code-generation options.
/// It is deserialized from the <c>nextnet.config.json</c> file at application startup
/// and registered as a singleton in the dependency injection container.
/// </para>
/// <example>
/// <code>
/// {
///   "data": {
///     "defaultConnection": "Default",
///     "connections": {
///       "Default": {
///         "connectionString": "Server=.;Database=MyApp;...",
///         "provider": "EntityFramework"
///       }
///     },
///     "migration": {
///       "autoApply": true,
///       "directory": "Migrations"
///     },
///     "scaffolding": {
///       "modelsNamespace": "App.Models"
///     }
///   }
/// }
/// </code>
/// </example>
/// </remarks>
/// <param name="DefaultConnection">The name of the default connection to use when no specific connection is requested. Defaults to <c>"Default"</c>.</param>
/// <param name="Connections">A dictionary of named connection configurations. Each key is a logical connection name.</param>
/// <param name="Migration">Optional migration-specific configuration, or <c>null</c> to use defaults.</param>
/// <param name="Scaffolding">Optional scaffolding-specific configuration, or <c>null</c> to use defaults.</param>
public sealed record DataConfig(
    [property: JsonPropertyName("defaultConnection")]
    string DefaultConnection = "Default",

    [property: JsonPropertyName("connections")]
    IReadOnlyDictionary<string, ConnectionConfig>? Connections = null,

    [property: JsonPropertyName("migration")]
    MigrationConfig? Migration = null,

    [property: JsonPropertyName("scaffolding")]
    ScaffoldingConfig? Scaffolding = null
);
