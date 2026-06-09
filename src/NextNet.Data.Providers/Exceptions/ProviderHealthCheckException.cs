namespace NextNet.Data.Exceptions;

/// <summary>
/// Thrown when a provider health check operation throws an unhandled exception.
/// Error code: <see cref="DataProviderErrorCodes.ProviderNotSupported"/> (DS-614).
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when <see cref="IDataProvider.IsHealthyAsync"/> throws
/// an exception instead of returning a <see cref="DataProviderHealthResult"/>. It
/// wraps the original exception for diagnostic purposes.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var result = await provider.IsHealthyAsync(ct);
/// }
/// catch (ProviderHealthCheckException ex) when (ex.ErrorCode == DataProviderErrorCodes.ProviderNotSupported)
/// {
///     Console.WriteLine($"Health check threw: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderHealthCheckException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderHealthCheckException"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider whose health check failed.</param>
    /// <param name="innerException">The exception that caused the health check failure.</param>
    public ProviderHealthCheckException(string providerName, Exception innerException)
        : base(DataProviderErrorCodes.ProviderNotSupported, $"Health check failed for provider '{providerName}'.", innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the provider whose health check failed.
    /// </summary>
    public string ProviderName { get; }
}
