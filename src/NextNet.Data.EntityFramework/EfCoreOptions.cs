using System.Reflection;

namespace NextNet.Data.EntityFramework;

/// <summary>
/// Provider-specific options for configuring the EF Core data provider.
/// Passed to <c>UseEntityFramework()</c> via the setup delegate.
/// </summary>
/// <remarks>
/// <para>
/// These options configure the <see cref="DbContextOptionsBuilder"/> used to create
/// <see cref="AppDbContext"/> instances. At minimum, a database provider must be configured
/// (e.g., <c>UseSqlite()</c>, <c>UseSqlServer()</c>, <c>UseNpgsql()</c>).
/// </para>
/// <para>
/// The <see cref="ConfigureDbContext"/> delegate is required. Without it, EF Core will not
/// know which database provider to use.
/// </para>
/// </remarks>
public sealed record EfCoreOptions
{
    /// <summary>
    /// Gets or sets a delegate to configure the <see cref="DbContextOptionsBuilder"/>.
    /// This is where the database provider and connection string are registered:
    /// <c>options.UseSqlite(connectionString)</c> or <c>options.UseNpgsql(connectionString)</c>.
    /// </summary>
    /// <remarks>
    /// This delegate is required. Without it, EF Core will not know which database provider to use.
    /// </remarks>
    public Action<DbContextOptionsBuilder>? ConfigureDbContext { get; set; }

    /// <summary>
    /// Gets or sets the name of the connection string from <see cref="DataConfig.Connections"/>
    /// that this provider instance should use. Defaults to <c>"Default"</c>.
    /// </summary>
    public string ConnectionName { get; set; } = "Default";

    /// <summary>
    /// Gets or sets the assemblies to scan for entity type configurations.
    /// If not set, the entry assembly is used.
    /// </summary>
    public IReadOnlyList<Assembly>? EntityConfigurationAssemblies { get; set; }

    /// <summary>
    /// Gets or sets whether to register default health checks. Defaults to <c>true</c>.
    /// </summary>
    public bool RegisterHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the service lifetime for generated repositories. Defaults to <see cref="ServiceLifetime.Scoped"/>.
    /// </summary>
    public ServiceLifetime RepositoryLifetime { get; set; } = ServiceLifetime.Scoped;

    /// <summary>
    /// Gets or sets whether to automatically apply migrations on application startup.
    /// If not set, falls back to <see cref="MigrationConfig.AutoApply"/>.
    /// </summary>
    public bool? AutoApplyMigrations { get; set; }

    /// <summary>
    /// Gets or sets the maximum retry count for transient EF Core errors. Defaults to 3.
    /// Set to 0 to disable retries.
    /// </summary>
    public int MaxRetryCount { get; set; } = 3;

    /// <summary>
    /// Gets or sets the enablement of sensitive data logging in EF Core.
    /// Defaults to <c>false</c>. Enable only in development.
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; }
}
