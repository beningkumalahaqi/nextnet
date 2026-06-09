using NextNet.Data.Exceptions;

namespace NextNet.Data.MultiDb.Exceptions;

/// <summary>
/// Exception thrown when a requested named connection is not registered.
/// Error code: DS-550.
/// </summary>
/// <remarks>
/// <para>
/// Thrown by <see cref="IDatabaseSelector.For"/> and <see cref="IDatabaseSelector.GetConnection"/>
/// when the specified connection name does not exist in the registry.
/// Ensure the connection is configured in <c>nextnet.config.json</c> under
/// <c>data.connections</c> or registered via <c>AddNamedProvider()</c>.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var ctx = db.For("NonExistentDb");
/// }
/// catch (MissingConnectionException ex) when (ex.ErrorCode == "DS-550")
/// {
///     Console.WriteLine(ex.Message);
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class MissingConnectionException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MissingConnectionException"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection that was not found.</param>
    public MissingConnectionException(string connectionName)
        : base("DS-550",
            $"No database connection registered with name '{connectionName}'. " +
            $"Ensure the connection is configured in nextnet.config.json under 'data.connections' " +
            $"or registered via AddNamedProvider().")
    {
        ConnectionName = connectionName;
    }

    /// <summary>
    /// Gets the name of the connection that was requested but not found.
    /// </summary>
    public string ConnectionName { get; }
}
