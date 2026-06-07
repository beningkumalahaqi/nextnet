using NextNet.Data.Exceptions;

namespace NextNet.Data.MultiDb.Exceptions;

/// <summary>
/// Exception thrown when a connection's configured provider does not match
/// the expected or registered provider type.
/// Error code: SKDATA_MULTIDB_004.
/// </summary>
/// <remarks>
/// <para>
/// Thrown during connection resolution when the provider specified in the
/// connection configuration (e.g., "EntityFramework") does not match an
/// available registered provider, or when attempting to use a repository
/// type with a provider that does not support it.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var ctx = db.For("Analytics");
///     var repo = ctx.GetRepository&lt;MyEntity&gt;();
/// }
/// catch (ProviderMismatchException ex) when (ex.ErrorCode == "SKDATA_MULTIDB_004")
/// {
///     Console.WriteLine($"Provider mismatch: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderMismatchException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderMismatchException"/> class.
    /// </summary>
    /// <param name="connectionName">The name of the connection.</param>
    /// <param name="expectedProvider">The expected provider name or type.</param>
    /// <param name="actualProvider">The actual provider name or type that was found.</param>
    /// <param name="message">A human-readable description of the mismatch.</param>
    public ProviderMismatchException(string connectionName, string expectedProvider, string actualProvider, string message)
        : base("SKDATA_MULTIDB_004",
            $"Provider mismatch for connection '{connectionName}': {message} " +
            $"(expected: {expectedProvider}, actual: {actualProvider})")
    {
        ConnectionName = connectionName;
        ExpectedProvider = expectedProvider;
        ActualProvider = actualProvider;
    }

    /// <summary>
    /// Gets the name of the connection where the mismatch occurred.
    /// </summary>
    public string ConnectionName { get; }

    /// <summary>
    /// Gets the expected provider name or type.
    /// </summary>
    public string ExpectedProvider { get; }

    /// <summary>
    /// Gets the actual provider name or type that was found.
    /// </summary>
    public string ActualProvider { get; }
}
