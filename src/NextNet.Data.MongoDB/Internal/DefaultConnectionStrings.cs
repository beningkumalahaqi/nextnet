using NextNet.Data.Exceptions;

namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Provides validation and database name resolution for MongoDB connection strings.
/// </summary>
/// <remarks>
/// <para>
/// MongoDB connection URIs follow the format:
/// <c>mongodb://[username:password@]host[:port][/database][?options]</c>
/// </para>
/// <para>
/// This helper validates connection strings and resolves the database name
/// from the URI path, falling back to the configured default.
/// </para>
/// </remarks>
internal static class DefaultConnectionStrings
{
    /// <summary>
    /// Validates that a MongoDB connection string is well-formed.
    /// </summary>
    /// <param name="connectionString">The connection string to validate.</param>
    /// <returns><c>true</c> if the connection string is valid; otherwise <c>false</c>.</returns>
    public static bool IsValid(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return false;
        }

        try
        {
            _ = MongoUrl.Create(connectionString);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves the database name from a connection string and optional default.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection URI.</param>
    /// <param name="defaultDatabaseName">The fallback database name if not specified in the URI.</param>
    /// <returns>The resolved database name, or <c>null</c> if neither source specifies one.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is not a valid MongoDB URI.</exception>
    public static string? ResolveDatabaseName(string connectionString, string? defaultDatabaseName = null)
    {
        MongoUrl url;
        try
        {
            url = MongoUrl.Create(connectionString);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(
                $"The connection string is not a valid MongoDB URI: {ex.Message}", nameof(connectionString), ex);
        }

        return url.DatabaseName ?? defaultDatabaseName;
    }

    /// <summary>
    /// Throws if the connection string is invalid or neither the URI nor the default specifies a database name.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection URI.</param>
    /// <param name="defaultDatabaseName">The fallback database name.</param>
    /// <param name="connectionName">The logical connection name for error messages.</param>
    /// <exception cref="ProviderConfigurationException">Thrown when validation fails.</exception>
    public static void Validate(string connectionString, string? defaultDatabaseName, string connectionName)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ProviderConfigurationException(
                connectionName,
                $"Connection string for '{connectionName}' is null or empty. " +
                "Provide a valid MongoDB connection URI or configure it in nextnet.config.json.");
        }

        if (!IsValid(connectionString))
        {
            throw new ProviderConfigurationException(
                connectionName,
                $"Connection string for '{connectionName}' is not a valid MongoDB URI. " +
                "Expected format: mongodb://[username:password@]host[:port][/database][?options]");
        }

        var resolvedDbName = ResolveDatabaseName(connectionString, defaultDatabaseName);
        if (string.IsNullOrWhiteSpace(resolvedDbName))
        {
            throw new ProviderConfigurationException(
                connectionName,
                $"No database name specified for connection '{connectionName}'. " +
                "Include a database name in the connection URI (e.g., mongodb://localhost:27017/mydb) " +
                "or set MongoDbOptions.DefaultDatabaseName.");
        }
    }
}
