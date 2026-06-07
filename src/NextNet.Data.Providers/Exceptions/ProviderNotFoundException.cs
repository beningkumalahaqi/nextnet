namespace NextNet.Data.Exceptions;

/// <summary>
/// Thrown when a requested provider is not found in the <see cref="IDataProviderRegistry"/>.
/// Error code: SKDATA_PROVIDER_003.
/// </summary>
/// <remarks>
/// <para>
/// This exception is thrown when attempting to resolve a provider by name from the
/// <see cref="IDataProviderRegistry"/> and no provider with the given name is registered.
/// </para>
/// <example>
/// <code>
/// try
/// {
///     var provider = registry.GetByName("NonExistentProvider");
///     if (provider is null)
///         throw new ProviderNotFoundException("NonExistentProvider");
/// }
/// catch (ProviderNotFoundException ex) when (ex.ErrorCode == "SKDATA_PROVIDER_003")
/// {
///     Console.WriteLine($"Provider not found: {ex.Message}");
/// }
/// </code>
/// </example>
/// </remarks>
public sealed class ProviderNotFoundException : NextNetDataException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderNotFoundException"/> class.
    /// </summary>
    /// <param name="providerName">The name of the provider that was not found.</param>
    public ProviderNotFoundException(string providerName)
        : base("SKDATA_PROVIDER_003", $"No provider registered with name '{providerName}'.")
    {
        ProviderName = providerName;
    }

    /// <summary>
    /// Gets the name of the provider that was not found.
    /// </summary>
    public string ProviderName { get; }
}
