using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Configuration;
using NextNet.Data.Abstractions.MultiDb;

namespace NextNet.Data.Abstractions.Registration;

/// <summary>
/// Fluent builder for configuring the NextNet data layer on an <see cref="IServiceCollection"/>.
/// Entry point: <c>services.AddNextNetData()</c>.
/// </summary>
/// <remarks>
/// <para>
/// The builder is the primary API surface for application developers to register
/// data providers, configure connections, and customize data layer behavior.
/// </para>
/// <para>
/// The builder implements <see cref="IDisposable"/> to support the <c>using</c> pattern,
/// which automatically calls <see cref="Build"/> when the builder is disposed.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .UseProvider&lt;EntityFrameworkProvider&gt;()
///     .WithConnection("Default", "Server=.;Database=MyApp;...")
///     .WithMigrationOptions(opts => opts = opts with { AutoApply = true })
///     .WithScaffoldingOptions(opts => opts = opts with { ModelsNamespace = "App.Models" })
///     .Build();
/// </code>
/// </example>
/// </remarks>
public sealed class NextNetDataBuilder : IDisposable
{
    private readonly IServiceCollection _services;
    private DataConfig _config;
    private bool _built;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetDataBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public NextNetDataBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
        _config = new DataConfig();
    }

    /// <summary>
    /// Registers a data provider of the specified type.
    /// The provider type must implement <see cref="IDataProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The provider implementation type.</typeparam>
    /// <param name="lifetime">The service lifetime. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder UseProvider<TProvider>(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TProvider : class, IDataProvider
    {
        ThrowIfBuilt();
        _services.Add(ServiceDescriptor.Describe(typeof(IDataProvider), typeof(TProvider), lifetime));
        return this;
    }

    /// <summary>
    /// Registers a data provider with an explicit factory method.
    /// </summary>
    /// <param name="factory">A factory delegate that creates the provider instance.</param>
    /// <param name="lifetime">The service lifetime. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder UseProvider(Func<IServiceProvider, IDataProvider> factory, ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        ThrowIfBuilt();
        ArgumentNullException.ThrowIfNull(factory);
        _services.Add(ServiceDescriptor.Describe(typeof(IDataProvider), factory, lifetime));
        return this;
    }

    /// <summary>
    /// Adds a named connection configuration with the specified connection string.
    /// </summary>
    /// <param name="name">The logical connection name (e.g., "Default", "Analytics").</param>
    /// <param name="connectionString">The database connection string.</param>
    /// <param name="provider">Optional provider name override for this connection.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> or <paramref name="connectionString"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder WithConnection(string name, string connectionString, string? provider = null)
    {
        ThrowIfBuilt();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"[{DataAbstractionsErrorCodes.ConfigurationInvalid}] Connection name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException($"[{DataAbstractionsErrorCodes.ConfigurationInvalid}] Connection string must not be null or empty.", nameof(connectionString));

        var config = new ConnectionConfig(
            ConnectionString: connectionString,
            Provider: provider ?? "EntityFramework");

        return WithConnection(name, config);
    }

    /// <summary>
    /// Adds a named connection from a <see cref="ConnectionConfig"/> instance.
    /// </summary>
    /// <param name="name">The logical connection name.</param>
    /// <param name="config">The connection configuration.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder WithConnection(string name, ConnectionConfig config)
    {
        ThrowIfBuilt();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException($"[{DataAbstractionsErrorCodes.ConfigurationInvalid}] Connection name must not be null or empty.", nameof(name));

        ArgumentNullException.ThrowIfNull(config);

        var connections = _config.Connections is not null
            ? new Dictionary<string, ConnectionConfig>(_config.Connections)
            : new Dictionary<string, ConnectionConfig>();

        connections[name] = config;

        _config = _config with { Connections = connections };
        return this;
    }

    /// <summary>
    /// Configures migration options.
    /// </summary>
    /// <param name="configure">A delegate to configure the <see cref="MigrationConfig"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder WithMigrationOptions(Func<MigrationConfig, MigrationConfig> configure)
    {
        ThrowIfBuilt();
        ArgumentNullException.ThrowIfNull(configure);
        _config = _config with { Migration = configure(_config.Migration ?? new MigrationConfig()) };
        return this;
    }

    /// <summary>
    /// Configures scaffolding/code generation options.
    /// </summary>
    /// <param name="configure">A delegate to configure the <see cref="ScaffoldingConfig"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configure"/> is <c>null</c>.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder WithScaffoldingOptions(Func<ScaffoldingConfig, ScaffoldingConfig> configure)
    {
        ThrowIfBuilt();
        ArgumentNullException.ThrowIfNull(configure);
        _config = _config with { Scaffolding = configure(_config.Scaffolding ?? new ScaffoldingConfig()) };
        return this;
    }

    /// <summary>
    /// Configures the default connection name used when no specific connection is specified.
    /// </summary>
    /// <param name="connectionName">The name of the default connection.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionName"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder WithDefaultConnection(string connectionName)
    {
        ThrowIfBuilt();

        if (string.IsNullOrWhiteSpace(connectionName))
            throw new ArgumentException($"[{DataAbstractionsErrorCodes.ConfigurationInvalid}] Default connection name must not be null or empty.", nameof(connectionName));

        _config = _config with { DefaultConnection = connectionName };
        return this;
    }

    /// <summary>
    /// Registers the generic <see cref="IRepository{T}"/> service for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity type. Must be a reference type.</typeparam>
    /// <param name="connectionName">Optional connection name to associate with this repository.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="InvalidOperationException">Thrown if <c>Build()</c> has already been called.</exception>
    public NextNetDataBuilder AddRepository<TEntity>(string? connectionName = null)
        where TEntity : class
    {
        ThrowIfBuilt();
        // Repository resolution is deferred to the provider; for now we register
        // a marker so the provider can wire up its own IRepository<T> implementation.
        _services.TryAddTransient(typeof(IRepository<TEntity>), sp =>
        {
            var provider = sp.GetRequiredService<IDataProvider>();
            // Providers can implement a factory method or use their own resolution.
            throw new InvalidOperationException(
                $"[{DataAbstractionsErrorCodes.QueryExecutionFailed}] IRepository<{typeof(TEntity).Name}> must be configured by the registered data provider. " +
                "Ensure the provider supports automatic repository registration.");
        });
        return this;
    }

    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Builds and finalizes the data layer registration.
    /// Validates the configuration, registers <see cref="DataConfig"/> as a singleton,
    /// and registers <see cref="IDatabaseSelector"/> as a singleton.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <c>Build()</c> has already been called, or when configuration validation fails.
    /// </exception>
    public void Build()
    {
        if (_built)
            throw new InvalidOperationException(
                $"[{DataAbstractionsErrorCodes.BuilderAlreadyBuilt}] Build() has already been called. The builder can only be used once.");

        // Validate configuration
        var validator = new DataConfigValidator();
        var errors = validator.Validate(_config);

        if (errors.Count > 0)
        {
            var errorMessage = string.Join("; ", errors);
            throw new InvalidOperationException(
                $"[{DataAbstractionsErrorCodes.ConfigurationInvalid}] Data configuration validation failed: {errorMessage}");
        }

        // Register DataConfig as singleton
        _services.AddSingleton(_config);

        // Register IDatabaseSelector as singleton
        _services.AddSingleton<IDatabaseSelector>(sp =>
            new DatabaseSelector(_config));

        _built = true;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// Calls <see cref="Build"/> if it has not already been called.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (!_built)
            {
                Build();
            }
        }
    }

    private void ThrowIfBuilt()
    {
        if (_built)
            throw new InvalidOperationException(
                $"[{DataAbstractionsErrorCodes.BuilderAlreadyBuilt}] Cannot modify the builder after Build() has been called.");
    }
}
