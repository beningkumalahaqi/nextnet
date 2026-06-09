using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NextNet.Data.EntityFramework;
using NextNet.Data.Sqlite.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="NextNetDataBuilder"/> to configure the EF Core provider
/// to use the SQLite database engine.
/// </summary>
/// <remarks>
/// <para>
/// This method must be called after <c>UseEntityFramework()</c> on the builder chain.
/// It registers the <see cref="SqliteConnectionFactory"/> and configures the DbContext options
/// to use <c>UseSqlite()</c> from EF Core's SQLite provider.
/// </para>
/// <para>
/// If no connection string is provided, the extension reads the connection string from the
/// <c>"data.connections.default"</c> section of <c>nextnet.config.json</c>. If neither exists,
/// it falls back to <c>"Data Source=database.db"</c> in the current directory.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Program.cs
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UseSqlite();  // uses config or defaults
///
/// // With explicit connection string:
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UseSqlite("Data Source=myapp.db;Cache=Shared");
///
/// // With options delegate:
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UseSqlite(options =>
///     {
///         options.DataSource = "custom.db";
///         options.Cache = SqliteCacheMode.Shared;
///     });
/// </code>
/// </example>
public static class NextNetDataBuilderExtensions
{
    /// <summary>
    /// Configures the EF Core provider to use SQLite.
    /// Registers <see cref="SqliteConnectionFactory"/> and configures DbContextOptions
    /// with <c>UseSqlite()</c>.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="connectionString">
    /// Optional explicit connection string. Overrides config and defaults.
    /// </param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder UseSqlite(
        this NextNetDataBuilder builder,
        string? connectionString = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new SqliteConnectionFactoryOptions();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.ConnectionString = connectionString;
        }

        ApplySqlite(builder, options);
        return builder;
    }

    /// <summary>
    /// Configures the EF Core provider to use SQLite with the specified options.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="configure">A delegate to configure <see cref="SqliteConnectionFactoryOptions"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder UseSqlite(
        this NextNetDataBuilder builder,
        Action<SqliteConnectionFactoryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new SqliteConnectionFactoryOptions();
        configure(options);

        ApplySqlite(builder, options);
        return builder;
    }

    /// <summary>
    /// Configures the EF Core provider to use an in-memory SQLite database.
    /// Equivalent to calling <c>UseSqlite()</c> with <c>options.InMemory = true</c>.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder UseInMemorySqlite(
        this NextNetDataBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new SqliteConnectionFactoryOptions
        {
            InMemory = true
        };

        ApplySqlite(builder, options);
        return builder;
    }

    /// <summary>
    /// Core registration logic: resolves connection string, registers services,
    /// and wires up the DbContext to use SQLite.
    /// </summary>
    private static void ApplySqlite(NextNetDataBuilder builder, SqliteConnectionFactoryOptions options)
    {
        // Resolve connection string immediately for DbContext configuration
        var resolver = new ConnectionStringResolver(options);
        var connectionString = resolver.Resolve();

        // Register options for IOptions<T> consumption by SqliteConnectionFactory
        builder.Services.AddSingleton<IOptions<SqliteConnectionFactoryOptions>>(
            new OptionsWrapper<SqliteConnectionFactoryOptions>(options));

        // Also register raw options for direct resolution
        builder.Services.AddSingleton(options);

        // Register the connection factory as singleton
        builder.Services.AddSingleton<SqliteConnectionFactory>();

        // Configure the DbContext to use SQLite by modifying EfCoreOptions
        ApplySqliteToDbContext(builder.Services, connectionString);
    }

    /// <summary>
    /// Applies SQLite configuration to the EF Core DbContext options.
    /// Wraps the existing <see cref="EfCoreOptions.ConfigureDbContext"/> delegate
    /// registered by <c>UseEntityFramework()</c> to chain in SQLite configuration.
    /// </summary>
    private static void ApplySqliteToDbContext(IServiceCollection services, string connectionString)
    {
        // Find the EfCoreOptions singleton registered by UseEntityFramework()
        var optionsDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(EfCoreOptions) &&
            d.Lifetime == ServiceLifetime.Singleton &&
            d.ImplementationInstance is EfCoreOptions);

        if (optionsDescriptor?.ImplementationInstance is not EfCoreOptions efOptions)
        {
            throw new InvalidOperationException(
                "[DS-534] The EF Core provider must be registered via UseEntityFramework() before calling UseSqlite(). " +
                "Ensure UseEntityFramework() is called before UseSqlite() in the builder chain.");
        }

        // Wrap the existing ConfigureDbContext delegate to also configure SQLite
        var previousConfigure = efOptions.ConfigureDbContext;
        efOptions.ConfigureDbContext = dbContextBuilder =>
        {
            // First invoke any previously configured delegate (e.g., user-provided options)
            previousConfigure?.Invoke(dbContextBuilder);

            // Then configure SQLite using the resolved connection string
            SqliteDbContextOptionsConfigurator.Configure(dbContextBuilder, connectionString);
        };
    }
}
