using NextNet.Data;
using NextNet.Data.EntityFramework;
using NextNet.Data.EntityFramework.Internal;
using Microsoft.Extensions.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering the EF Core data provider in the NextNet data pipeline.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods provide the primary way to configure Entity Framework Core
/// as the data provider. Call <c>UseEntityFramework()</c> after <c>AddNextNetData()</c>
/// to set up DbContext, repositories, migrations, and health checks.
/// </para>
/// </remarks>
public static class EntityFrameworkNextNetDataExtensions
{
    /// <summary>
    /// Registers Entity Framework Core as the data provider with the given configuration.
    /// </summary>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance from <c>AddNextNetData()</c>.</param>
    /// <param name="configure">An optional delegate to configure <see cref="EfCoreOptions"/>.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData()
    ///     .UseEntityFramework(options =>
    ///     {
    ///         options.ConfigureDbContext = db => db.UseSqlite(connectionString);
    ///         options.ConnectionName = "Default";
    ///         options.AutoApplyMigrations = true;
    ///     })
    ///     .AddRepository&lt;User&gt;()
    ///     .AddRepository&lt;Product&gt;();
    /// </code>
    /// </example>
    public static NextNetDataBuilder UseEntityFramework(
        this NextNetDataBuilder builder,
        Action<EfCoreOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var options = new EfCoreOptions();
        configure?.Invoke(options);

        builder.Services.AddSingleton(options);

        // Configure DbContext options with EF Core settings
        builder.Services.AddDbContextFactory<AppDbContext>(dbContextOptionsBuilder =>
        {
            options.ConfigureDbContext?.Invoke(dbContextOptionsBuilder);

            // Apply sensitive data logging (for development purposes)
            if (options.EnableSensitiveDataLogging)
            {
                dbContextOptionsBuilder.EnableSensitiveDataLogging();
            }
        });

        // Set entity configuration assemblies on AppDbContext
        if (options.EntityConfigurationAssemblies is not null)
        {
            AppDbContext.EntityConfigurationAssemblies = options.EntityConfigurationAssemblies;
        }

        // Register the provider
        builder.AddProvider<EfCoreDataProvider>("EntityFramework", opts =>
        {
            opts.RegisterHealthChecks = options.RegisterHealthChecks;
        });

        // Register the migration engine
        builder.Services.AddSingleton<IMigrationEngine>(sp =>
        {
            var contextFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var logger = sp.GetService<ILogger<EfCoreMigrationEngine>>();
            var migrationConfig = sp.GetService<Microsoft.Extensions.Options.IOptions<MigrationConfig>>();

            return new EfCoreMigrationEngine(
                contextFactory,
                migrationConfig?.Value,
                logger);
        });

        // Register the health check provider
        builder.Services.AddSingleton<IHealthCheckProvider>(sp =>
        {
            var contextFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
            var logger = sp.GetService<ILogger<EfCoreHealthCheckProvider>>();
            var efOptions = sp.GetRequiredService<EfCoreOptions>();

            return new EfCoreHealthCheckProvider(
                contextFactory,
                new[] { efOptions.ConnectionName },
                logger);
        });

        // Register auto-migration hosted service if enabled
        if (options.AutoApplyMigrations == true)
        {
            builder.Services.AddSingleton<IHostedService>(sp =>
            {
                var contextFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
                var efOptions = sp.GetRequiredService<EfCoreOptions>();
                var migrationConfig = sp.GetService<Microsoft.Extensions.Options.IOptions<MigrationConfig>>();
                var logger = sp.GetService<ILogger<AutoMigrationHostedService>>();

                return new AutoMigrationHostedService(
                    contextFactory,
                    efOptions,
                    migrationConfig?.Value,
                    logger);
            });
        }

        return builder;
    }

    /// <summary>
    /// Registers a repository for the specified entity type using <see cref="EfCoreRepository{T}"/>.
    /// Repositories are registered with <see cref="ServiceLifetime.Scoped"/> lifetime by default,
    /// matching EF Core's recommended scoped DbContext pattern.
    /// </summary>
    /// <typeparam name="TEntity">The entity type for the repository.</typeparam>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetData()
    ///     .UseEntityFramework(opts => { ... })
    ///     .AddRepository&lt;User&gt;()
    ///     .AddRepository&lt;Product&gt;();
    /// </code>
    /// </example>
    public static NextNetDataBuilder AddRepository<TEntity>(
        this NextNetDataBuilder builder)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddScoped<IRepository<TEntity>>(sp =>
        {
            var contextFactory = sp.GetRequiredService<IDbContextFactory<AppDbContext>>();
            return new EfCoreRepository<TEntity>(contextFactory);
        });

        return builder;
    }
}
