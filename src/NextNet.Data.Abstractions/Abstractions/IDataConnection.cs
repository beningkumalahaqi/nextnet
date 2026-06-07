using NextNet.Data.Abstractions.Configuration;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Represents a named database connection managed by a NextNet data provider.
/// Connections are configured via <see cref="ConnectionConfig"/> and resolved
/// through the <see cref="MultiDb.IDatabaseSelector"/> for multi-database scenarios.
/// </summary>
/// <remarks>
/// <para>
/// Each <see cref="IDataConnection"/> wraps a resolved connection string, its logical name,
/// and the provider that owns the connection. Instances are created by the provider
/// and cached for the application lifetime.
/// </para>
/// <example>
/// <code>
/// public class MyService
/// {
///     private readonly IDataConnection _connection;
///
///     public MyService(IDataConnection connection)
///     {
///         _connection = connection;
///     }
///
///     public string GetConnectionInfo() =>
///         $"Connected to {_connection.Name} via {_connection.ProviderName}";
/// }
/// </code>
/// </example>
/// </remarks>
public interface IDataConnection
{
    /// <summary>
    /// Gets the logical name of this connection (e.g., "Default", "Analytics", "Logging").
    /// Matches the key used in <see cref="DataConfig.Connections"/>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the resolved connection string for this connection.
    /// May be sourced from configuration, environment variables, or secret stores.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets the name of the data provider that owns this connection
    /// (e.g., "EntityFramework", "Dapper").
    /// </summary>
    string ProviderName { get; }
}
