namespace NextNet.Data.Dapper.Internal;

/// <summary>
/// Builds connection strings with pool settings from <see cref="DapperOptions"/>.
/// </summary>
/// <remarks>
/// <para>
/// This internal utility constructs <see cref="SqlConnectionStringBuilder"/> instances
/// with the provider's pool configuration applied on top of the base connection string.
/// </para>
/// </remarks>
internal static class DefaultConnectionStrings
{
    /// <summary>
    /// Builds a connection string from the base string applying pool settings.
    /// </summary>
    /// <param name="baseConnectionString">The base connection string from configuration.</param>
    /// <param name="options">The Dapper provider options with pool settings.</param>
    /// <returns>A connection string with pool properties applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="baseConnectionString"/> is null or empty.</exception>
    internal static string BuildConnectionString(string baseConnectionString, DapperOptions options)
    {
        ArgumentException.ThrowIfNullOrEmpty(baseConnectionString);

        var builder = new SqlConnectionStringBuilder(baseConnectionString)
        {
            Pooling = options.EnablePooling,
            MinPoolSize = options.MinPoolSize,
            MaxPoolSize = options.MaxPoolSize,
            ConnectTimeout = options.CommandTimeoutSeconds
        };

        if (options.ConnectionLifetimeSeconds > 0)
        {
            builder.LoadBalanceTimeout = options.ConnectionLifetimeSeconds;
        }

        return builder.ConnectionString;
    }
}
