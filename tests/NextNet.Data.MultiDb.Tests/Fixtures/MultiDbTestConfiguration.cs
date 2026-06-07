using NextNet.Data.MultiDb.Internal;

namespace NextNet.Data.MultiDb.Tests.Fixtures;

/// <summary>
/// Helper for building test configurations for multi-database tests.
/// Provides factory methods to create common test arrangements.
/// </summary>
internal static class MultiDbTestConfiguration
{
    /// <summary>
    /// Creates a <see cref="ConnectionNameRegistry"/> pre-populated with test connections.
    /// </summary>
    /// <param name="connections">Collection of (name, providerName, connectionString) tuples.</param>
    /// <returns>A populated registry.</returns>
    public static ConnectionNameRegistry CreateNameRegistry(params (string Name, string Provider, string ConnectionString)[] connections)
    {
        var registry = new ConnectionNameRegistry();
        foreach (var (name, provider, cs) in connections)
        {
            registry.Register(name, new ConnectionRegistration(
                ConnectionName: name,
                ProviderName: provider,
                ConnectionString: cs,
                ProviderType: typeof(FakeDataProvider),
                IsInitialized: true));
        }
        return registry;
    }

    /// <summary>
    /// Creates a <see cref="ConnectionPoolRegistry"/> pre-populated with test pool entries.
    /// </summary>
    /// <param name="connections">Collection of (name, providerName, connectionString) tuples.</param>
    /// <returns>A populated registry.</returns>
    public static ConnectionPoolRegistry CreatePoolRegistry(params (string Name, string Provider, string ConnectionString)[] connections)
    {
        var registry = new ConnectionPoolRegistry();
        foreach (var (name, providerName, cs) in connections)
        {
            registry.Register(name, new ConnectionPoolEntry(
                ConnectionName: name,
                ProviderName: providerName,
                ConnectionString: cs,
                Provider: new FakeDataProvider(name),
                IsEnabled: true));
        }
        return registry;
    }

    /// <summary>
    /// Creates a configured <see cref="MultiDbOptions"/> instance.
    /// </summary>
    /// <param name="validateOnStartup">Whether to validate on startup.</param>
    /// <param name="cacheContexts">Whether to cache contexts.</param>
    /// <param name="fallbackToDefault">Whether to fall back to default.</param>
    /// <returns>The options instance.</returns>
    public static IOptions<MultiDbOptions> CreateOptions(
        bool validateOnStartup = true,
        bool cacheContexts = true,
        bool fallbackToDefault = false)
    {
        return Options.Create(new MultiDbOptions
        {
            ValidateOnStartup = validateOnStartup,
            CacheContexts = cacheContexts,
            FallbackToDefault = fallbackToDefault
        });
    }

    /// <summary>
    /// Creates a complete test setup with registries, options, and a service provider.
    /// </summary>
    /// <returns>A tuple with all components wired together.</returns>
    public static (ConnectionNameRegistry NameRegistry, ConnectionPoolRegistry PoolRegistry, IOptions<MultiDbOptions> Options, IServiceProvider ServiceProvider) CreateFullSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var sp = services.BuildServiceProvider();

        var nameRegistry = CreateNameRegistry(
            ("Primary", "EntityFramework", "Server=primary;Database=Test"),
            ("Analytics", "Dapper", "Host=analytics;Database=Reports"),
            ("Logging", "EntityFramework", "Data Source=logs.db"));

        var poolRegistry = CreatePoolRegistry(
            ("Primary", "EntityFramework", "Server=primary;Database=Test"),
            ("Analytics", "Dapper", "Host=analytics;Database=Reports"),
            ("Logging", "EntityFramework", "Data Source=logs.db"));

        var options = CreateOptions();

        return (nameRegistry, poolRegistry, options, sp);
    }
}
