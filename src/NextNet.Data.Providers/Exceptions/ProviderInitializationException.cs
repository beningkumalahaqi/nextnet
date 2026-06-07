namespace NextNet.Data.Exceptions;

/// <summary>
/// Thrown when a data provider fails to initialize during application startup.
/// Error code: SKDATA_PROVIDER_002.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="Internal.ProviderInitializationHostedService"/>
/// when <see cref="IDataProvider.InitializeAsync"/> throws an exception and
/// <see cref="DataAbstractionsOptions.FailOnInitializationError"/> is <c>true</c>.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     await provider.InitializeAsync(ct);
/// }
/// catch (ProviderInitializationException ex) when (ex.ErrorCode == "SKDATA_PROVIDER_002")
/// {
///     Console.WriteLine($"Provider failed to start: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderInitializationException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderInitializationException"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider that failed to initialize.</param>
    /// <param name="innerException">The exception that caused the initialization failure.</param>
    public ProviderInitializationException(string providerName, Exception innerException)
        : base("SKDATA_PROVIDER_002", $"Provider '{providerName}' initialization failed.", innerException)
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the provider that failed to initialize.
    /// </summary>
    public string ProviderName { get; }
}
