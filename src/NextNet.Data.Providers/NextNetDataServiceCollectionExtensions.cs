using NextNet.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> for registering the NextNet data layer.
/// Entry point for configuring providers via the fluent <see cref="NextNetDataBuilder"/> API.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide the primary entry point for NextNet data provider
/// registration. Call <c>services.AddNextNetData()</c> at application startup to
/// begin the fluent configuration chain.
/// </para>
/// <example>
/// <code>
/// // Simple registration
/// builder.Services
///     .AddNextNetData()
///     .AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework",
///         opts => opts.ConnectionStringName = "Default");
///
/// // With configuration delegate
/// builder.Services.AddNextNetData(options =>
/// {
///     options.FailOnInitializationError = false;
/// });
/// </code>
/// </example>
/// </remarks>
public static class NextNetDataServiceCollectionExtensions
{
    /// <summary>
    /// Registers NextNet Data services and returns a <see cref="NextNetDataBuilder"/>
    /// for fluent provider configuration.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional delegate to configure <see cref="DataAbstractionsOptions"/>.</param>
    /// <returns>A <see cref="NextNetDataBuilder"/> for chaining provider registrations.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// builder.Services
    ///     .AddNextNetData()
    ///     .AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework",
    ///         options => options.ConnectionStringName = "Default");
    /// </code>
    /// </example>
    public static NextNetDataBuilder AddNextNetData(
        this IServiceCollection services,
        Action<DataAbstractionsOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new DataAbstractionsOptions();
        configure?.Invoke(options);

        return new NextNetDataBuilder(services, options);
    }

    /// <summary>
    /// Registers NextNet Data services with an <see cref="IServiceCollection"/> and immediately
    /// invokes <see cref="NextNetDataBuilder.Build"/>. Convenience overload for simple setups.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureBuilder">An action to configure the <see cref="NextNetDataBuilder"/>.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="DataAbstractionsOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for further chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="services"/> or <paramref name="configureBuilder"/> is <c>null</c>.
    /// </exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData(builder =>
    /// {
    ///     builder.AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework");
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetData(
        this IServiceCollection services,
        Action<NextNetDataBuilder> configureBuilder,
        Action<DataAbstractionsOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureBuilder);

        var options = new DataAbstractionsOptions();
        configureOptions?.Invoke(options);

        var builder = new NextNetDataBuilder(services, options);
        configureBuilder(builder);
        builder.Build();

        return services;
    }
}
