namespace NextNet.Data.Sdk;

/// <summary>
/// Marks a class as a NextNet data provider implementation.
/// The provider name is derived from the class name by stripping the
/// "DataProvider" suffix (e.g., <c>MyCustomDataProvider</c> → <c>"MyCustom"</c>).
/// Override with the <see cref="Name"/> property.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to the main provider class. The SDK tooling uses this
/// attribute for discovery, analyzer validation, and CLI integration.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [DataProvider(Name = "MyCustomDb", Description = "A custom database provider")]
/// public class MyCustomDataProvider : DataProviderBase
/// {
///     // Provider implementation
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DataProviderAttribute : Attribute
{
    /// <summary>
    /// Gets or sets a custom provider name. Defaults to the class name without "DataProvider" suffix.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets a human-readable display name for the provider.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a human-readable description of the provider.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the provider version. Defaults to "0.1.0".
    /// </summary>
    public string? Version { get; set; }
}
