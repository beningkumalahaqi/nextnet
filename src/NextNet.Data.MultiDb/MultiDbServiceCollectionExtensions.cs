using NextNet.Data;
using NextNet.Data.MultiDb;
using NextNet.Data.MultiDb.Exceptions;
using NextNet.Data.MultiDb.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="NextNetDataBuilder"/> for configuring
/// multi-database support, including named connections and the selector.
/// </summary>
/// <remarks>
/// <para>
/// These extensions provide the fluent API for registering multi-database
/// support on the <see cref="NextNetDataBuilder"/>. Call <c>WithDatabaseSelector()</c>
/// to enable the selector, and <c>WithDatabase()</c> to register individual
/// named connections.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework")
///     .WithDatabaseSelector(opts =>
///     {
///         opts.ValidateOnStartup = true;
///         opts.CacheContexts = true;
///     })
///     .WithDatabase("Primary", "Server=.;...")
///     .WithDatabase("Analytics", "Host=...;Database=Reports");
/// </code>
/// </example>
/// </remarks>
public static class MultiDbServiceCollectionExtensions
{
    /// <summary>
    /// Enables multi-database support by registering the <see cref="IDatabaseSelector"/>
    /// and its internal dependencies (<see cref="ConnectionNameRegistry"/>,
    /// <see cref="ConnectionPoolRegistry"/>, etc.) in the DI container.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="configure">An optional delegate to configure <see cref="MultiDbOptions"/>.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder WithDatabaseSelector(
        this NextNetDataBuilder builder,
        Action<MultiDbOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Configure options
        var options = new MultiDbOptions();
        configure?.Invoke(options);

        // Register options
        builder.Services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));

        // Register connection store for WithDatabase() registrations
        builder.Services.AddSingleton(new List<ConnectionConfig>());

        // Register internal registries
        builder.Services.AddSingleton<ConnectionNameRegistry>();
        builder.Services.AddSingleton<ConnectionPoolRegistry>();

        // Register context factory
        builder.Services.AddTransient<DatabaseContextFactory>();

        // Register the selector
        builder.Services.AddSingleton<IDatabaseSelector>(sp =>
        {
            var nameRegistry = sp.GetRequiredService<ConnectionNameRegistry>();
            var poolRegistry = sp.GetRequiredService<ConnectionPoolRegistry>();
            var contextFactory = sp.GetRequiredService<DatabaseContextFactory>();
            var opts = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<MultiDbOptions>>();
            var logger = sp.GetRequiredService<ILogger<DatabaseSelector>>();

            // Populate registries from connection configs registered via WithDatabase()
            var connectionConfigs = sp.GetService<List<ConnectionConfig>>();
            if (connectionConfigs is not null)
            {
                // Resolve providers registered via AddProvider/AddNamedProvider
                var abstractionProviders = sp.GetServices<NextNet.Data.Abstractions.Abstractions.IDataProvider>();

                foreach (var connConfig in connectionConfigs)
                {
                    var connectionName = connConfig.Name ?? connConfig.Provider;
                    var providerName = connConfig.Provider;
                    var provider = abstractionProviders.FirstOrDefault(p =>
                        string.Equals(p.Name, providerName, StringComparison.OrdinalIgnoreCase));

                    // Register in name registry (keyed by the logical connection name)
                    nameRegistry.Register(connectionName, new ConnectionRegistration(
                        ConnectionName: connectionName,
                        ProviderName: providerName,
                        ConnectionString: connConfig.ConnectionString,
                        ProviderType: provider?.GetType() ?? typeof(object),
                        IsInitialized: provider is not null));

                    // Register in pool registry (keyed by the logical connection name)
                    var poolEntry = new ConnectionPoolEntry(
                        ConnectionName: connectionName,
                        ProviderName: providerName,
                        ConnectionString: connConfig.ConnectionString,
                        Provider: provider ?? new LazyProviderPlaceholder(providerName),
                        IsEnabled: connConfig.Enabled);

                    poolRegistry.Register(connectionName, poolEntry);
                }
            }

            return new DatabaseSelector(nameRegistry, poolRegistry, contextFactory, opts, logger);
        });

        // Register guard for startup validation if enabled
        if (options.ValidateOnStartup)
        {
            builder.Services.AddSingleton<DatabaseSelectorGuard>();
        }

        return builder;
    }

    /// <summary>
    /// Registers a named database connection with the selector.
    /// The connection will be resolved by name at runtime via <c>selector.For("name")</c>.
    /// Must be called after <see cref="WithDatabaseSelector"/>.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="name">The logical connection name (e.g., "Analytics", "Primary").</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="providerName">Optional provider name override. If not set, uses "EntityFramework".</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> or <paramref name="connectionString"/> is null or empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="WithDatabaseSelector"/> has not been called before this method.
    /// </exception>
    /// <exception cref="ConnectionNameConflictException">
    /// Thrown when a connection with the same name is already registered.
    /// </exception>
    public static NextNetDataBuilder WithDatabase(
        this NextNetDataBuilder builder,
        string name,
        string connectionString,
        string? providerName = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Connection name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        // Create a ConnectionConfig for this database
        var config = new ConnectionConfig(
            ConnectionString: connectionString,
            Provider: providerName ?? "EntityFramework")
        {
            Name = name
        };

        // Find the connection configs list registered by WithDatabaseSelector()
        // We look for it in the service collection by checking service descriptors
        var configs = GetOrCreateConnectionConfigs(builder.Services);

        if (configs.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConnectionNameConflictException(name);
        }

        configs.Add(config);

        return builder;
    }

    /// <summary>
    /// Gets or creates the connection configs list from the service collection.
    /// </summary>
    private static List<ConnectionConfig> GetOrCreateConnectionConfigs(IServiceCollection services)
    {
        // Check if the list is already registered (by WithDatabaseSelector)
        for (var i = 0; i < services.Count; i++)
        {
            var sd = services[i];
            if (sd.ServiceType == typeof(List<ConnectionConfig>) && sd.Lifetime == ServiceLifetime.Singleton)
            {
                if (sd.ImplementationInstance is List<ConnectionConfig> existing)
                {
                    return existing;
                }

                // If registered via factory, we can't access it yet.
                // Return a new list that will be reconciled later.
                // This is a simplified approach - in production, WithDatabase
                // should always be called after WithDatabaseSelector.
                throw new InvalidOperationException(
                    "[DS-555] Connection configs are registered via a factory. " +
                    "Ensure WithDatabaseSelector() is called before WithDatabase().");
            }
        }

        // If not registered yet, create a new list and register it
        // This allows WithDatabase to work even before WithDatabaseSelector is called
        var configs = new List<ConnectionConfig>();
        services.AddSingleton(configs);
        return configs;
    }

    /// <summary>
    /// Placeholder provider used when a provider instance hasn't been resolved yet.
    /// </summary>
    private sealed class LazyProviderPlaceholder : NextNet.Data.Abstractions.Abstractions.IDataProvider
    {
        public LazyProviderPlaceholder(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task InitializeAsync(DataConfig config, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<HealthCheckResult> IsHealthyAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HealthCheckResult(false, "Provider not yet initialized", TimeSpan.Zero));
        }
    }
}
