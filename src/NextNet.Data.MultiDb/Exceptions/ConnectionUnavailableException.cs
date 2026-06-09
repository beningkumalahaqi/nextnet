using NextNet.Data.Exceptions;

namespace NextNet.Data.MultiDb.Exceptions;

/// <summary>
/// Exception thrown when a connection's provider is not initialized or unavailable.
/// Error code: DS-552.
/// </summary>
/// <remarks>
/// <para>
/// Thrown when a named connection exists in the registry but its provider is
/// not initialized, the connection is disabled, or the pool entry is missing.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var ctx = db.For("DisabledConnection");
/// }
/// catch (ConnectionUnavailableException ex) when (ex.ErrorCode == "DS-552")
/// {
///     Console.WriteLine($"Connection unavailable: {ex.Reason}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ConnectionUnavailableException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionUnavailableException"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the unavailable connection.</param>
    /// <param name="providerName">The name of the provider for the connection.</param>
    /// <param name="reason">A description of why the connection is unavailable.</param>
    public ConnectionUnavailableException(string connectionName, string providerName, string reason)
        : base("DS-552",
            $"Connection '{connectionName}' (provider: {providerName}) is unavailable: {reason}")
    {
        ConnectionName = connectionName;
        ProviderName = providerName;
        Reason = reason;
    }

    /// <summary>
    /// Gets the name of the unavailable connection.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the name of the provider for the connection.
    /// </summary>
    public string ProviderName { get; }

    /// <summary>
    /// Gets the reason why the connection is unavailable.
    /// </summary>
    public string Reason { get; }
}
