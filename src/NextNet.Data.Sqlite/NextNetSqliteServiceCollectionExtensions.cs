using Microsoft.Extensions.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Service collection extensions for registering SQLite services independently.
/// Used internally by <c>UseSqlite()</c> and available for advanced scenarios where
/// users need custom DI registration ordering.
/// </summary>
/// <remarks>
/// <para>
/// This extension method registers the <see cref="SqliteConnectionFactory"/> as a singleton
/// and binds <see cref="SqliteConnectionFactoryOptions"/> for configuration.
/// It is called internally by <c>UseSqlite()</c> on the <c>NextNetDataBuilder</c>.
/// </para>
/// </remarks>
public static class NextNetSqliteServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="SqliteConnectionFactory"/> and SQLite configuration services.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">Optional delegate to configure <see cref="SqliteConnectionFactoryOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// services.AddNextNetSqlite(options =>
    /// {
    ///     options.DataSource = "app.db";
    ///     options.Cache = SqliteCacheMode.Shared;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetSqlite(
        this IServiceCollection services,
        Action<SqliteConnectionFactoryOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Build options and apply configuration delegate
        var options = new SqliteConnectionFactoryOptions();
        configure?.Invoke(options);

        // Register for IOptions<T> resolution
        services.AddSingleton<IOptions<SqliteConnectionFactoryOptions>>(
            new OptionsWrapper<SqliteConnectionFactoryOptions>(options));

        // Register raw options for direct resolution
        services.AddSingleton(options);

        // Register the connection factory as singleton
        services.AddSingleton<SqliteConnectionFactory>();

        return services;
    }
}
