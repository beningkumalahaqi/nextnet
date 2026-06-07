using NextNet.Data;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.MultiDb.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="NextNetDataBuilder"/> for registering
/// named providers and connection-scoped repositories in a multi-database setup.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable the full multi-database registration pattern:
/// provider registration with named connections, and repository registration
/// scoped to specific database connections.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .AddNamedProvider&lt;DapperDataProvider&gt;("Analytics", "Host=...")
///     .AddRepository&lt;User&gt;()                             // Default connection
///     .AddRepository&lt;SalesSummary&gt;("Analytics");          // Scoped to Analytics
/// </code>
/// </example>
/// </remarks>
public static class MultiDbNextNetDataExtensions
{
    /// <summary>
    /// Registers a provider for a named connection (multi-provider / multi-database scenarios).
    /// Each named provider instance is registered independently with its own connection string,
    /// options, and service registrations.
    /// </summary>
    /// <typeparam name="TProvider">The concrete <see cref="NextNet.Data.IDataProvider"/> type.</typeparam>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="connectionName">The logical connection name (e.g., "Analytics", "Primary", "Logging").</param>
    /// <param name="connectionString">The connection string for this named instance.</param>
    /// <param name="setup">An optional action to configure provider-specific options.</param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="connectionName"/> or <paramref name="connectionString"/> is null or empty.
    /// </exception>
    public static NextNetDataBuilder AddNamedProvider<TProvider>(
        this NextNetDataBuilder builder,
        string connectionName,
        string connectionString,
        Action<ProviderRegistrationOptions>? setup = null)
        where TProvider : class, NextNet.Data.IDataProvider
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Delegate to the existing AddNamedProvider method on NextNetDataBuilder
        return builder.AddNamedProvider<TProvider>(connectionName, connectionString, setup);
    }

    /// <summary>
    /// Registers the generic <see cref="IRepository{T}"/> service for the specified entity type,
    /// optionally scoped to a named connection.
    /// </summary>
    /// <typeparam name="TEntity">The entity type. Must be a class.</typeparam>
    /// <param name="builder">The <see cref="NextNetDataBuilder"/> instance.</param>
    /// <param name="connectionName">
    /// The logical connection name to associate with this repository.
    /// If <c>null</c>, the default connection is used.
    /// </param>
    /// <returns>The builder instance for further chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    public static NextNetDataBuilder AddRepository<TEntity>(
        this NextNetDataBuilder builder,
        string? connectionName = null)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Register a transient repository that resolves from the appropriate connection
        builder.Services.AddTransient<IRepository<TEntity>>(sp =>
        {
            var selector = sp.GetRequiredService<IDatabaseSelector>();

            IDatabaseContext context;
            if (!string.IsNullOrWhiteSpace(connectionName))
            {
                context = selector.For(connectionName);
            }
            else
            {
                context = selector.Default;
            }

            return context.GetRepository<TEntity>();
        });

        return builder;
    }
}
