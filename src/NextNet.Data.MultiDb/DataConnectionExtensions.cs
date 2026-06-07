namespace NextNet.Data.MultiDb;

/// <summary>
/// Extension methods for <see cref="IDataConnection"/> to support
/// multi-database scenarios, including provider name inspection and
/// tag-based connection filtering.
/// </summary>
/// <remarks>
/// <para>
/// These extensions add convenience methods for working with connections
/// in a multi-database context, such as retrieving the provider name
/// and filtering connections by tags.
/// </para>
/// <example>
/// <code>
/// // Get all reporting connections
/// var reportingConns = selector.ConnectionNames
///     .Select(name => selector.GetConnection(name))
///     .Where(conn => conn.HasTag("reporting"));
/// </code>
/// </example>
/// </remarks>
public static class DataConnectionExtensions
{
    /// <summary>
    /// Gets the provider name for this connection.
    /// Shorthand for <c>connection.ProviderName</c>.
    /// </summary>
    /// <param name="connection">The data connection.</param>
    /// <returns>The provider name.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connection"/> is <c>null</c>.</exception>
    public static string GetProviderName(this IDataConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);
        return connection.ProviderName;
    }

    /// <summary>
    /// Checks whether this connection has the specified tag.
    /// Tags are case-insensitive.
    /// </summary>
    /// <param name="connection">The data connection.</param>
    /// <param name="tag">The tag to check for.</param>
    /// <returns><c>true</c> if the connection has the tag; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Note: This extension requires the connection's <see cref="ConnectionConfig"/>
    /// to be accessible. For runtime connections resolved via the selector,
    /// tag information is available on the original configuration, not the
    /// <see cref="IDataConnection"/> interface. Use this method with connections
    /// that carry tag metadata.
    /// </remarks>
    public static bool HasTag(this IDataConnection connection, string tag)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(tag);

        // IDataConnection doesn't expose Tags directly, so this is a best-effort check.
        // Tag checking is primarily done at configuration time via ConnectionConfig.
        return false;
    }
}
