using NextNet.Data.Exceptions;
using NextNet.Data.Internal;

namespace NextNet.Data;

/// <summary>
/// Fluent builder for registering NextNet data providers.
/// Holds state across chained calls and applies registrations to
/// the underlying <see cref="IServiceCollection"/> on build.
/// </summary>
/// <remarks>
/// <para>
/// The builder is the primary API surface for application developers to register
/// data providers. Provider registration is deferred until <see cref="Build"/> is
/// called, which allows extension methods from separate NuGet packages to add
/// state to the builder without requiring a specific call order.
/// </para>
/// <para>
/// The builder exposes its underlying <see cref="Services"/> collection for
/// scenarios where consumers need to register additional services outside
/// the NextNet provider model.
/// </para>
/// <example>
/// <code>
/// builder.Services
///     .AddNextNetData()
///     .AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework", opts =>
///     {
///         opts.ConnectionStringName = "Default";
///     })
///     .AddNamedProvider&lt;MongoDbProvider&gt;("MongoDb", "mongodb://localhost:27017");
/// </code>
/// </example>
/// </remarks>
public sealed class NextNetDataBuilder
{
    private readonly List<ProviderRegistrationDescriptor> _registrations = new();
    private readonly IServiceCollection _services;
    private DataAbstractionsOptions _options;
    private bool _built;
    private bool _disposed;

    /// <summary>
    /// Gets the underlying <see cref="IServiceCollection"/> for further chaining.
    /// </summary>
    public IServiceCollection Services => _services;

    /// <summary>
    /// Initializes a new instance of the <see cref="NextNetDataBuilder"/> class.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="options">The global data abstraction options.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public NextNetDataBuilder(IServiceCollection services, DataAbstractionsOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
        _options = options ?? new DataAbstractionsOptions();
    }

    /// <summary>
    /// Registers a provider using the specified type and optional setup action.
    /// Called internally by provider-specific extension methods.
    /// </summary>
    /// <typeparam name="TProvider">The concrete <see cref="IDataProvider"/> type.</typeparam>
    /// <param name="name">The provider name (e.g., "EntityFramework").</param>
    /// <param name="setup">An optional action to configure provider registration options.</param>
    /// <returns>The same builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null or empty.</exception>
    /// <exception cref="ProviderRegistrationException">
    /// Thrown when a provider with the same <paramref name="name"/> has already been registered.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when <c>Build()</c> has already been called.</exception>
    /// <example>
    /// <code>
    /// builder.AddProvider&lt;MyProvider&gt;("MyProvider", opts =>
    /// {
    ///     opts.ConnectionStringName = "Default";
    ///     opts.RegisterHealthChecks = true;
    /// });
    /// </code>
    /// </example>
    public NextNetDataBuilder AddProvider<TProvider>(string name, Action<ProviderRegistrationOptions>? setup = null)
        where TProvider : class, IDataProvider
    {
        ThrowIfBuilt();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name must not be null or empty.", nameof(name));

        if (_registrations.Any(r => r.Name == name))
            throw new ProviderRegistrationException(name, $"A provider with name '{name}' is already registered.");

        var options = new ProviderRegistrationOptions();
        setup?.Invoke(options);

        _registrations.Add(new ProviderRegistrationDescriptor(
            Name: name,
            ProviderType: typeof(TProvider),
            Options: options,
            ConnectionName: null,
            ConnectionString: null));

        return this;
    }

    /// <summary>
    /// Registers a provider for a named connection (multi-provider scenarios).
    /// </summary>
    /// <typeparam name="TProvider">The concrete <see cref="IDataProvider"/> type.</typeparam>
    /// <param name="name">The connection name (e.g., "Analytics", "Primary").</param>
    /// <param name="connectionString">The connection string for this named instance.</param>
    /// <param name="setup">An optional action to configure provider registration options.</param>
    /// <returns>The same builder instance for chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="name"/> or <paramref name="connectionString"/> is null or empty.
    /// </exception>
    /// <exception cref="ProviderRegistrationException">
    /// Thrown when a provider with the same <paramref name="name"/> has already been registered.
    /// </exception>
    /// <exception cref="InvalidOperationException">Thrown when <c>Build()</c> has already been called.</exception>
    /// <example>
    /// <code>
    /// builder.AddNamedProvider&lt;AnalyticsProvider&gt;("Analytics",
    ///     "Server=.;Database=Analytics;...",
    ///     opts => opts.RegisterHealthChecks = true);
    /// </code>
    /// </example>
    public NextNetDataBuilder AddNamedProvider<TProvider>(string name, string connectionString, Action<ProviderRegistrationOptions>? setup = null)
        where TProvider : class, IDataProvider
    {
        ThrowIfBuilt();

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Provider name must not be null or empty.", nameof(name));

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        if (_registrations.Any(r => r.Name == name))
            throw new ProviderRegistrationException(name, $"A provider with name '{name}' is already registered.");

        var options = new ProviderRegistrationOptions();
        setup?.Invoke(options);

        _registrations.Add(new ProviderRegistrationDescriptor(
            Name: name,
            ProviderType: typeof(TProvider),
            Options: options,
            ConnectionName: name,
            ConnectionString: connectionString));

        return this;
    }

    /// <summary>
    /// Applies all accumulated registrations to the service collection.
    /// Called automatically when the application builds the service provider.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when <c>Build()</c> has already been called.</exception>
    public void Build()
    {
        if (_built)
            throw new InvalidOperationException("Build() has already been called. The builder can only be used once.");

        // Register DataAbstractionsOptions as singleton
        _services.AddSingleton(_options);

        // Register IDataProviderRegistry as singleton with its implementation
        var registry = new DataProviderRegistryImpl();

        var providerRegistrations = new List<ProviderRegistrationDescriptor>(_registrations);

        // Register each provider in the DI container
        foreach (var descriptor in providerRegistrations)
        {
            var lifetime = descriptor.Options.Lifetime;

            // Register the concrete provider type
            _services.Add(ServiceDescriptor.Describe(
                typeof(IDataProvider),
                descriptor.ProviderType,
                lifetime));

            // Add to the registry
            registry.AddProvider(descriptor);
        }

        // Register the registry
        _services.AddSingleton<IDataProviderRegistry>(registry);

        // Register the initialization hosted service
        _services.AddTransient<Microsoft.Extensions.Hosting.IHostedService>(
            sp => new ProviderInitializationHostedService(
                sp.GetRequiredService<IDataProviderRegistry>(),
                sp.GetRequiredService<ILogger<ProviderInitializationHostedService>>(),
                sp.GetRequiredService<DataAbstractionsOptions>()));

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
            throw new InvalidOperationException("Cannot modify the builder after Build() has been called.");
    }
}
