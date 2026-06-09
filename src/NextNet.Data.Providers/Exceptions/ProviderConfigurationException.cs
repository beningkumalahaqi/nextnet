namespace NextNet.Data.Exceptions;

/// <summary>
/// Thrown when required provider configuration is missing or invalid.
/// Error code: <see cref="DataProviderErrorCodes.InvalidProviderConfiguration"/> (DS-612).
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when a provider's registration options are incomplete
/// or contain invalid values, such as a missing connection string when one is required.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     builder.AddProvider&lt;MyProvider&gt;("MyProvider", opts =>
///     {
///         // Missing required connection string configuration
///     });
/// }
/// catch (ProviderConfigurationException ex) when (ex.ErrorCode == DataProviderErrorCodes.InvalidProviderConfiguration)
/// {
///     Console.WriteLine($"Configuration error: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderConfigurationException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderConfigurationException"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider with the configuration error.</param>
    /// <param name="detail">A description of the configuration problem.</param>
    public ProviderConfigurationException(string providerName, string detail)
        : base(DataProviderErrorCodes.InvalidProviderConfiguration, $"Provider '{providerName}' configuration error: {detail}")
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the provider with the configuration error.
    /// </summary>
    public string ProviderName { get; }
}
