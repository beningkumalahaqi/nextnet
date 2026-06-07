namespace NextNet.Data.Exceptions;

/// <summary>
/// Thrown when attempting to register a provider with a name that is already registered.
/// Error code: SKDATA_PROVIDER_001.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown by <see cref="NextNetDataBuilder.AddProvider{TProvider}"/>
/// and <see cref="NextNetDataBuilder.AddNamedProvider{TProvider}"/> when a duplicate
/// provider name is detected. Provider names must be unique within an application.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     builder.AddProvider&lt;MyProvider&gt;("DuplicateName");
///     builder.AddProvider&lt;AnotherProvider&gt;("DuplicateName");
/// }
/// catch (ProviderRegistrationException ex) when (ex.ErrorCode == "SKDATA_PROVIDER_001")
/// {
///     Console.WriteLine($"Registration error: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderRegistrationException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderRegistrationException"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider that caused the conflict.</param>
    /// <param name="message">A human-readable description of the registration error.</param>
    public ProviderRegistrationException(string providerName, string message)
        : base("SKDATA_PROVIDER_001", $"Provider '{providerName}': {message}")
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the provider that caused the registration conflict.
    /// </summary>
    public string ProviderName { get; }
}
