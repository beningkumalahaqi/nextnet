namespace NextNet.Data.Abstractions.Configuration;

/// <summary>
/// Validates <see cref="DataConfig"/> instances and returns descriptive error messages.
/// </summary>
/// <remarks>
/// <para>
/// The validator checks all configuration rules defined for the data layer, including
/// required fields, range constraints, cross-references between properties, and
/// provider-specific requirements.
/// </para>
/// <example>
/// <code>
/// var validator = new DataConfigValidator();
/// var config = new DataConfig(DefaultConnection: "", Connections: null);
/// var errors = validator.Validate(config);
/// // errors contains: "DefaultConnection must not be null or empty."
/// </code>
/// </example>
/// </remarks>
public sealed class DataConfigValidator
{
    /// <summary>
    /// Validates the specified <see cref="DataConfig"/> and returns any validation errors.
    /// </summary>
    /// <param name="config">The configuration to validate. Must not be <c>null</c>.</param>
    /// <returns>A list of validation error messages. Empty if the configuration is valid.</returns>
    /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="config"/> is <c>null</c>.</exception>
    public IReadOnlyList<string> Validate(DataConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = new List<string>();

        // DefaultConnection must not be null or empty
        if (string.IsNullOrWhiteSpace(config.DefaultConnection))
        {
            errors.Add("DefaultConnection must not be null or empty.");
        }

        // Validate each connection entry
        if (config.Connections is { Count: > 0 })
        {
            foreach (var kvp in config.Connections)
            {
                // Each key must be non-empty
                if (string.IsNullOrWhiteSpace(kvp.Key))
                {
                    errors.Add("Connection key must not be null or empty.");
                }

                ValidateConnectionConfig(kvp.Key, kvp.Value, errors);
            }

            // DefaultConnection must reference a key in Connections when Connections is non-empty
            if (!string.IsNullOrWhiteSpace(config.DefaultConnection) &&
                !config.Connections.ContainsKey(config.DefaultConnection))
            {
                errors.Add(
                    $"DefaultConnection '{config.DefaultConnection}' does not match any key in the Connections dictionary.");
            }
        }

        // Validate Migration config
        if (config.Migration is not null)
        {
            ValidateMigrationConfig(config.Migration, errors);
        }

        // Validate Scaffolding config
        if (config.Scaffolding is not null)
        {
            ValidateScaffoldingConfig(config.Scaffolding, errors);
        }

        return errors.AsReadOnly();
    }

    private static void ValidateConnectionConfig(string connectionKey, ConnectionConfig connection, List<string> errors)
    {
        // ConnectionString must not be null or empty
        if (string.IsNullOrWhiteSpace(connection.ConnectionString))
        {
            errors.Add($"Connection '{connectionKey}': ConnectionString must not be null or empty.");
        }

        // Provider must not be null or empty
        if (string.IsNullOrWhiteSpace(connection.Provider))
        {
            errors.Add($"Connection '{connectionKey}': Provider must not be null or empty.");
        }

        // TimeoutSeconds must be between 1 and 3600
        if (connection.TimeoutSeconds < 1 || connection.TimeoutSeconds > 3600)
        {
            errors.Add(
                $"Connection '{connectionKey}': TimeoutSeconds ({connection.TimeoutSeconds}) must be between 1 and 3600.");
        }

        // PoolSize must be >= 1 if set
        if (connection.PoolSize is < 1)
        {
            errors.Add(
                $"Connection '{connectionKey}': PoolSize ({connection.PoolSize}) must be >= 1 when set.");
        }
    }

    private static void ValidateMigrationConfig(MigrationConfig migration, List<string> errors)
    {
        // Directory must not be null or empty
        if (string.IsNullOrWhiteSpace(migration.Directory))
        {
            errors.Add("Migration: Directory must not be null or empty.");
        }

        // HistoryTableName must not be null or empty
        if (string.IsNullOrWhiteSpace(migration.HistoryTableName))
        {
            errors.Add("Migration: HistoryTableName must not be null or empty.");
        }

        // TimeoutSeconds must be >= 1
        if (migration.TimeoutSeconds < 1)
        {
            errors.Add($"Migration: TimeoutSeconds ({migration.TimeoutSeconds}) must be >= 1.");
        }
    }

    private static void ValidateScaffoldingConfig(ScaffoldingConfig scaffolding, List<string> errors)
    {
        // ModelsNamespace must not be null or empty
        if (string.IsNullOrWhiteSpace(scaffolding.ModelsNamespace))
        {
            errors.Add("Scaffolding: ModelsNamespace must not be null or empty.");
        }

        // RepositoriesNamespace must not be null or empty
        if (string.IsNullOrWhiteSpace(scaffolding.RepositoriesNamespace))
        {
            errors.Add("Scaffolding: RepositoriesNamespace must not be null or empty.");
        }

        // ModelsDirectory must not be null or empty
        if (string.IsNullOrWhiteSpace(scaffolding.ModelsDirectory))
        {
            errors.Add("Scaffolding: ModelsDirectory must not be null or empty.");
        }

        // RepositoriesDirectory must not be null or empty
        if (string.IsNullOrWhiteSpace(scaffolding.RepositoriesDirectory))
        {
            errors.Add("Scaffolding: RepositoriesDirectory must not be null or empty.");
        }
    }
}
