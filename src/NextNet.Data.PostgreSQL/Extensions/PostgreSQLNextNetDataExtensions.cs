using Microsoft.EntityFrameworkCore;
using NextNet.Data.EntityFramework;
using NextNet.Data.PostgreSQL.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="NextNetDataBuilder"/> to configure the EF Core provider
/// to use the PostgreSQL database engine via Npgsql.
/// </summary>
/// <remarks>
/// <para>
/// This method must be called after <c>UseEntityFramework()</c> on the builder chain.
/// It registers the <see cref="PostgresConnectionFactory"/> and configures the DbContext
/// options to use <c>UseNpgsql()</c> from the Npgsql EF Core provider.
/// </para>
/// <para>
/// Connection string resolution follows this priority:
/// <list type="number">
///   <item>Explicit <c>connectionString</c> parameter</item>
///   <item>Options configured via <c>Action&lt;PostgresConnectionFactoryOptions&gt;</c></item>
///   <item>Environment variables: <c>PGHOST</c>, <c>PGPORT</c>, <c>PGDATABASE</c>, <c>PGUSER</c>, <c>PGPASSWORD</c></item>
///   <item><c>nextnet.config.json</c> → <c>data.connections.default.connectionString</c></item>
///   <item>Default: <c>Host=localhost;Port=5432;Database={project-name};Username=postgres</c></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Program.cs — Simple (uses config or defaults)
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UsePostgreSQL();
///
/// // With explicit connection string:
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UsePostgreSQL("Host=localhost;Port=5432;Database=myapp;Username=postgres;Password=secret");
///
/// // With options delegate (full control):
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UsePostgreSQL(options =>
///     {
///         options.Host = "db.example.com";
///         options.Port = 5432;
///         options.Database = "myapp_prod";
///         options.Username = "app_user";
///         options.Password = env["DB_PASSWORD"];
///         options.Ssl.Mode = PostgresSslMode.Require;
///         options.Pooling.MinPoolSize = 5;
///         options.Pooling.MaxPoolSize = 50;
///     });
///
/// // With environment variables (12-factor app):
/// // export PGHOST=localhost
/// // export PGPORT=5432
/// // export PGDATABASE=myapp
/// // export PGUSER=app_user
/// // export PGPASSWORD=secret
/// builder.Services.AddNextNetData()
///     .UseEntityFramework()
///     .UsePostgreSQL();  // auto-detects from env
/// </code>
/// </example>
public static class PostgreSQLNextNetDataExtensions
{
    /// <summary>
    /// Configures the EF Core provider to use PostgreSQL with Npgsql.
    /// Registers <see cref="PostgresConnectionFactory"/> and configures DbContextOptions
    /// with <c>UseNpgsql()</c>.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="connectionString">
    /// Optional explicit Npgsql connection string. Overrides config, environment, and defaults.
    /// </param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder UsePostgreSQL(
        this NextNetDataBuilder builder,
        string? connectionString = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new PostgresConnectionFactoryOptions();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            options.ConnectionString = connectionString;
        }

        ApplyPostgreSql(builder, options);
        return builder;
    }

    /// <summary>
    /// Configures the EF Core provider to use PostgreSQL with the specified options.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="configure">A delegate to configure <see cref="PostgresConnectionFactoryOptions"/>.</param>
    /// <returns>The builder instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> or <paramref name="configure"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder UsePostgreSQL(
        this NextNetDataBuilder builder,
        Action<PostgresConnectionFactoryOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new PostgresConnectionFactoryOptions();
        configure(options);

        ApplyPostgreSql(builder, options);
        return builder;
    }

    /// <summary>
    /// Core registration logic: resolves connection string, registers services,
    /// and wires up the DbContext to use PostgreSQL.
    /// </summary>
    private static void ApplyPostgreSql(NextNetDataBuilder builder, PostgresConnectionFactoryOptions options)
    {
        // Resolve connection string immediately for DbContext configuration
        var resolver = new ConnectionStringResolver(options);
        var connectionString = resolver.Resolve();

        // Register options for IOptions<T> consumption by PostgresConnectionFactory
        builder.Services.AddSingleton<IOptions<PostgresConnectionFactoryOptions>>(
            new OptionsWrapper<PostgresConnectionFactoryOptions>(options));

        // Also register raw options for direct resolution
        builder.Services.AddSingleton(options);

        // Register the connection factory as singleton
        builder.Services.AddSingleton<PostgresConnectionFactory>();

        // Configure the DbContext to use PostgreSQL by modifying EfCoreOptions
        ApplyPostgreSqlToDbContext(builder.Services, connectionString);
    }

    /// <summary>
    /// Applies PostgreSQL configuration to the EF Core DbContext options.
    /// Wraps the existing <see cref="EfCoreOptions.ConfigureDbContext"/> delegate
    /// registered by <c>UseEntityFramework()</c> to chain in PostgreSQL configuration.
    /// </summary>
    private static void ApplyPostgreSqlToDbContext(IServiceCollection services, string connectionString)
    {
        // Find the EfCoreOptions singleton registered by UseEntityFramework()
        var optionsDescriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(EfCoreOptions) &&
            d.Lifetime == ServiceLifetime.Singleton &&
            d.ImplementationInstance is EfCoreOptions);

        if (optionsDescriptor?.ImplementationInstance is not EfCoreOptions efOptions)
        {
            throw new InvalidOperationException(
                "[DS-517] The EF Core provider must be registered via UseEntityFramework() before calling UsePostgreSQL(). " +
                "Ensure UseEntityFramework() is called before UsePostgreSQL() in the builder chain.");
        }

        // Wrap the existing ConfigureDbContext delegate to also configure PostgreSQL
        var previousConfigure = efOptions.ConfigureDbContext;
        efOptions.ConfigureDbContext = dbContextBuilder =>
        {
            // First invoke any previously configured delegate (e.g., user-provided options)
            previousConfigure?.Invoke(dbContextBuilder);

            // Then configure PostgreSQL using the resolved connection string
            NpgsqlDbContextOptionsConfigurator.Configure(dbContextBuilder, connectionString);
        };
    }
}
