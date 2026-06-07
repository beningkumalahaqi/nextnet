using NextNet.Data.Exceptions;

namespace NextNet.Data.MultiDb.Exceptions;

/// <summary>
/// Exception thrown when a duplicate connection name is registered.
/// Error code: SKDATA_MULTIDB_002.
/// </summary>
/// <remarks>
/// <para>
/// Thrown during startup when multiple named provider registrations use the same
/// connection name. Connection names must be unique within a NextNet application.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     builder.AddNamedProvider&lt;MyProvider&gt;("Analytics", "cs1");
///     builder.AddNamedProvider&lt;OtherProvider&gt;("Analytics", "cs2"); // Throws
/// }
/// catch (ConnectionNameConflictException ex) when (ex.ErrorCode == "SKDATA_MULTIDB_002")
/// {
///     Console.WriteLine(ex.Message);
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ConnectionNameConflictException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionNameConflictException"/> class.
    /// </summary>
    /// <param name="connectionName">The duplicate connection name.</param>
    public ConnectionNameConflictException(string connectionName)
        : base("SKDATA_MULTIDB_002",
            $"A connection with name '{connectionName}' is already registered. " +
            $"Connection names must be unique within a NextNet application.")
    {
        ConnectionName = connectionName;
    }

    /// <summary>
    /// Gets the name of the connection that caused the conflict.
    /// </summary>
    public string ConnectionName { get; }
}
