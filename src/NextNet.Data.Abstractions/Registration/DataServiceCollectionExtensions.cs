using NextNet.Data.Abstractions.Registration;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> for registering the NextNet data layer.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide the entry point for configuring NextNet data providers,
/// connections, migrations, and scaffolding via the fluent <see cref="NextNetDataBuilder"/> API.
/// </para>
/// <example>
/// <code>
/// // Simple registration
/// builder.Services.AddNextNetData()
///     .UseProvider&lt;EntityFrameworkProvider&gt;()
///     .WithConnection("Default", configuration.GetConnectionString("Default"))
///     .Build();
///
/// // With configuration delegate
/// builder.Services.AddNextNetData(builder =>
/// {
///     builder.UseProvider&lt;DapperProvider&gt;()
///            .WithConnection("Default", "Server=.;...");
/// });
/// </code>
/// </example>
/// </remarks>
public static class DataServiceCollectionExtensions
{
    /// <summary>
    /// Begins the fluent registration of NextNet data services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <returns>A <see cref="NextNetDataBuilder"/> for chaining configuration calls.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData()
    ///     .UseProvider&lt;EntityFrameworkProvider&gt;()
    ///     .WithConnection("Default", configuration.GetConnectionString("Default"))
    ///     .Build();
    /// </code>
    /// </example>
    public static NextNetDataBuilder AddNextNetData(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return new NextNetDataBuilder(services);
    }

    /// <summary>
    /// Registers NextNet data services and immediately calls <see cref="NextNetDataBuilder.Build"/>.
    /// Convenience overload for simple setups.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configure">A delegate to configure the builder.</param>
    /// <returns>The <see cref="IServiceCollection"/> for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData(builder =>
    /// {
    ///     builder.UseProvider&lt;EntityFrameworkProvider&gt;()
    ///            .WithConnection("Default", configuration.GetConnectionString("Default"));
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetData(this IServiceCollection services, Action<NextNetDataBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new NextNetDataBuilder(services);
        configure(builder);
        builder.Build();

        return services;
    }
}
