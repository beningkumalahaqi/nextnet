using NextNet.Data.MongoDB;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering the MongoDB data provider in the NextNet data pipeline.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide the primary way to configure MongoDB as the data provider.
/// Call <c>UseMongoDB()</c> after <c>AddNextNetData()</c> to set up client management,
/// repositories, migrations, and health checks.
/// </para>
/// </remarks>
public static class MongoDbNextNetDataExtensions
{
    /// <summary>
    /// Registers MongoDB as the data provider with the given configuration.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance from <c>AddNextNetData()</c>.</param>
    /// <param name="configure">An optional delegate to configure <see cref="MongoDbOptions"/>.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <example>
    /// <code>
    /// // Minimal setup
    /// builder.Services.AddNextNetData()
    ///     .UseMongoDB();
    ///
    /// // With explicit configuration
    /// builder.Services.AddNextNetData()
    ///     .UseMongoDB(options =>
    ///     {
    ///         options.ConnectionName = "Default";
    ///         options.DefaultDatabaseName = "myapp";
    ///         options.MaxConnectionPoolSize = 50;
    ///         options.RetryWrites = true;
    ///     })
    ///     .AddRepository&lt;User&gt;()
    ///     .AddRepository&lt;Product&gt;();
    /// </code>
    /// </example>
    public static NextNetDataBuilder UseMongoDB(
        this NextNetDataBuilder builder,
        Action<MongoDbOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new MongoDbOptions();
        configure?.Invoke(options);

        builder.Services.AddSingleton(options);

        // Register the MongoDB provider
        builder.AddProvider<MongoDbProvider>("MongoDB", opts =>
        {
            opts.RegisterHealthChecks = options.RegisterHealthChecks;
        });

        // Register the client manager as singleton
        builder.Services.AddSingleton<MongoClientManager>(sp =>
        {
            var mongoOptions = sp.GetRequiredService<MongoDbOptions>();
            var config = sp.GetService<DataConfig>();
            var connections = config?.Connections ?? new Dictionary<string, ConnectionConfig>();
            var logger = sp.GetService<ILogger<MongoClientManager>>();

            return new MongoClientManager(
                connections,
                mongoOptions,
                logger: logger);
        });

        // Register the migration engine
        builder.Services.AddSingleton<IMigrationEngine>(sp =>
        {
            var clientManager = sp.GetRequiredService<MongoClientManager>();
            var mongoOptions = sp.GetRequiredService<MongoDbOptions>();
            var config = sp.GetService<DataConfig>();
            var logger = sp.GetService<ILogger<MongoDbMigrationEngine>>();

            return new MongoDbMigrationEngine(
                clientManager,
                config?.Migration,
                mongoOptions.ConnectionName,
                logger);
        });

        // Register the health check provider
        builder.Services.AddSingleton<IHealthCheckProvider>(sp =>
        {
            var clientManager = sp.GetRequiredService<MongoClientManager>();
            var mongoOptions = sp.GetRequiredService<MongoDbOptions>();
            var logger = sp.GetService<ILogger<MongoDbHealthCheck>>();

            return new MongoDbHealthCheck(
                clientManager,
                new[] { mongoOptions.ConnectionName },
                logger);
        });

        return builder;
    }
}
