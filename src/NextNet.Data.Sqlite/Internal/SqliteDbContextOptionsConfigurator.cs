using Microsoft.EntityFrameworkCore;

namespace NextNet.Data.Sqlite.Internal;

/// <summary>
/// Configures Entity Framework Core <see cref="DbContextOptionsBuilder"/> to use
/// the SQLite database provider via <c>UseSqlite()</c>.
/// </summary>
/// <remarks>
/// <para>
/// This is an internal configurator consumed by the registration extensions.
/// It delegates to EF Core's standard <c>UseSqlite()</c> configuration and applies
/// NextNet-specific conventions (migration history table name, assembly resolution).
/// </para>
/// </remarks>
internal static class SqliteDbContextOptionsConfigurator
{
    /// <summary>
    /// Configures the <see cref="DbContextOptionsBuilder"/> to use SQLite with the
    /// specified connection string.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder"/> to configure.</param>
    /// <param name="connectionString">The SQLite connection string to use.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="builder"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    public static void Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string must not be null or empty.", nameof(connectionString));

        builder.UseSqlite(connectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly(typeof(SqliteConnectionFactory).Assembly.FullName);
            sqliteOptions.MigrationsHistoryTable("__NextNetMigrations");
        });
    }
}
