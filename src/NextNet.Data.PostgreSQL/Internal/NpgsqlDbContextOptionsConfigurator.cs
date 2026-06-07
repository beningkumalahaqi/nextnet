using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace NextNet.Data.PostgreSQL.Internal;

/// <summary>
/// Configures Entity Framework Core <see cref="DbContextOptionsBuilder"/> to use
/// the PostgreSQL database provider via <c>UseNpgsql()</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is an internal configurator consumed by the registration extensions.
/// It delegates to EF Core's standard <c>UseNpgsql()</c> configuration and applies
/// NextNet-specific conventions (migration history table name, assembly resolution).
/// </para>
/// </remarks>
internal static class NpgsqlDbContextOptionsConfigurator
{
    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> to use Npgsql with the
    /// specified connection string.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder"/> to configure.</param>
    /// <param name="connectionString">The Npgsql connection string to use.</param>
    /// <param name="npgsqlOptions">
    /// An optional delegate for advanced Npgsql-specific configuration
    /// (e.g., <c>MigrationsHistoryTable</c>, <c>UseNetTopologySuite</c>).
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    public static void Configure(
        DbContextOptionsBuilder builder,
        string connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        builder.UseNpgsql(connectionString, npgsqlOptionsBuilder =>
        {
            npgsqlOptionsBuilder.MigrationsHistoryTable("__NextNetMigrations");

            // Apply any user-provided Npgsql configuration
            npgsqlOptions?.Invoke(npgsqlOptionsBuilder);
        });
    }
}
