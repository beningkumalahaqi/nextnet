// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extensions for registering PostgreSQL services independently.
/// Used internally by <c>UsePostgreSQL()</c> and available for advanced scenarios where
/// users need custom DI registration ordering.
/// </summary>
/// <remarks>
/// <para>
/// This extension method registers the <see cref="PostgresConnectionFactory"/> as a singleton
/// and binds <see cref="PostgresConnectionFactoryOptions"/> for configuration.
/// It is called internally by <c>UsePostgreSQL()</c> on the <c>NextNetDataBuilder</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddNextNetPostgreSql(options =>
/// {
///     options.Host = "db.example.com";
///     options.Database = "myapp";
///     options.Username = "app_user";
///     options.Password = "secret";
///     options.Ssl.Mode = PostgresSslMode.Require;
/// });
/// </code>
/// </example>
public static class NextNetPostgreSqlServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="PostgresConnectionFactory"/> and PostgreSQL configuration services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional delegate to configure <see cref="PostgresConnectionFactoryOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// services.AddNextNetPostgreSql(options =>
    /// {
    ///     options.Host = "localhost";
    ///     options.Database = "myapp";
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetPostgreSql(
        this IServiceCollection services,
        Action<PostgresConnectionFactoryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build options and apply configuration delegate
        var options = new PostgresConnectionFactoryOptions();
        configure?.Invoke(options);

        // Register for IOptions<T> resolution
        services.AddSingleton<IOptions<PostgresConnectionFactoryOptions>>(
            new OptionsWrapper<PostgresConnectionFactoryOptions>(options));

        // Register raw options for direct resolution
        services.AddSingleton(options);

        // Register the connection factory as singleton
        services.AddSingleton<PostgresConnectionFactory>();

        return services;
    }
}
