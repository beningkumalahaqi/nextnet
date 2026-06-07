namespace NextNet.Data;

/// <summary>
/// Global options for the NextNet data layer.
/// Configured via <see cref="NextNetDataServiceCollectionExtensions.AddNextNetData(IServiceCollection, Action{DataAbstractionsOptions}?)"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DataAbstractionsOptions"/> provides global settings that affect all
/// registered data providers. Options here control error handling behavior during
/// initialization and other cross-cutting concerns.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData(options =>
/// {
///     options.FailOnInitializationError = true;
/// });
/// </code>
/// </example>
/// </remarks>
public class DataAbstractionsOptions
{
    /// <summary>
    /// Gets or sets whether the application should fail to start when a provider
    /// initialization throws an exception.
    /// When <c>true</c> (default), the <see cref="Internal.ProviderInitializationHostedService"/>
    /// will throw, preventing the application from starting.
    /// When <c>false</c>, initialization errors are logged but the application continues.
    /// </summary>
    public bool FailOnInitializationError { get; set; } = true;
}
