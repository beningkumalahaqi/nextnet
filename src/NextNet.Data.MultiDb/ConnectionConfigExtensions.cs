namespace NextNet.Data.MultiDb;

/// <summary>
/// Fluent extension methods for <see cref="ConnectionConfig"/> to configure
/// multi-database specific properties like pool size and tags.
/// </summary>
/// <remarks>
/// <para>
/// These extensions provide a fluent API for enriching connection configurations
/// with multi-database metadata such as connection pool sizing and categorization tags.
/// </para>
/// <example>
/// <code>
/// var config = new ConnectionConfig("Server=...;")
///     .WithPoolSize(20)
///     .WithTags("readonly", "reporting");
/// </code>
/// </example>
/// </remarks>
public static class ConnectionConfigExtensions
{
    /// <summary>
    /// Sets the maximum pool size for this connection.
    /// </summary>
    /// <param name="config">The connection configuration.</param>
    /// <param name="poolSize">The maximum pool size. Must be &gt;= 1.</param>
    /// <returns>A new <see cref="ConnectionConfig"/> with the updated pool size.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="poolSize"/> is less than 1.</exception>
    public static ConnectionConfig WithPoolSize(this ConnectionConfig config, int poolSize)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (poolSize < 1)
            throw new ArgumentOutOfRangeException(nameof(poolSize), poolSize, "PoolSize must be >= 1.");

        return config with { PoolSize = poolSize };
    }

    /// <summary>
    /// Adds categorization tags to this connection.
    /// Tags enable logical grouping and runtime querying (e.g., "readonly", "reporting", "tenant:acme").
    /// </summary>
    /// <param name="config">The connection configuration.</param>
    /// <param name="tags">The tags to add. Empty or null tags are ignored.</param>
    /// <returns>A new <see cref="ConnectionConfig"/> with the updated tags.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    public static ConnectionConfig WithTags(this ConnectionConfig config, params string[]? tags)
    {
        ArgumentNullException.ThrowIfNull(config);

        var validTags = tags?.Where(t => !string.IsNullOrWhiteSpace(t)).ToList().AsReadOnly();
        return config with { Tags = validTags };
    }
}
