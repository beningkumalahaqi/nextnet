using NextNet.Data;
using NextNet.Data.Dapper;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering the Dapper data provider in the NextNet data pipeline.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide the primary way to configure Dapper as the data provider.
/// Call <c>UseDapper()</c> after <c>AddNextNetData()</c> to set up connection management,
/// repositories, migrations, and health checks.
/// </para>
/// </remarks>
public static class DapperNextNetDataExtensions
{
    /// <summary>
    /// Registers Dapper as the data provider with the given configuration.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance from <c>AddNextNetData()</c>.</param>
    /// <param name="configure">An optional delegate to configure <see cref="DapperOptions"/>.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData()
    ///     .UseDapper(options =>
    ///     {
    ///         options.ConnectionName = "Default";
    ///         options.CommandTimeoutSeconds = 60;
    ///     });
    /// </code>
    /// </example>
    public static NextNetDataBuilder UseDapper(
        this NextNetDataBuilder builder,
        Action<DapperOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new DapperOptions();
        configure?.Invoke(options);

        builder.Services.AddSingleton(options);

        // Register the Dapper provider
        builder.AddProvider<DapperDataProvider>("Dapper", opts =>
        {
            opts.RegisterHealthChecks = options.RegisterHealthChecks;
        });

        // Register the connection manager as singleton
        builder.Services.AddSingleton<DapperConnectionManager>(sp =>
        {
            var dapperOptions = sp.GetRequiredService<DapperOptions>();
            return new DapperConnectionManager(
                new Dictionary<string, ConnectionConfig>(),
                dapperOptions,
                sp.GetService<ILogger<DapperConnectionManager>>());
        });

        // Register the health check provider
        builder.Services.AddSingleton<IHealthCheckProvider>(sp =>
        {
            var connectionManager = sp.GetRequiredService<DapperConnectionManager>();
            var dapperOptions = sp.GetRequiredService<DapperOptions>();
            var logger = sp.GetService<ILogger<DapperHealthCheck>>();

            return new DapperHealthCheck(
                connectionManager,
                new[] { dapperOptions.ConnectionName },
                logger);
        });

        return builder;
    }
}
