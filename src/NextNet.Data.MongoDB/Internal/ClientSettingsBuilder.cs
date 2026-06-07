namespace NextNet.Data.MongoDB.Internal;

/// <summary>
/// Builds <see cref="MongoClientSettings"/> from a connection string and <see cref="MongoDbOptions"/>.
/// Applies pool sizing, timeouts, read preference, write concern, and retry settings.
/// </summary>
/// <remarks>
/// <para>
/// The builder starts from a MongoDB connection URI string and overlays
/// <see cref="MongoDbOptions"/> values on top. If the connection string already
/// specifies a setting, the options value takes precedence.
/// </para>
/// </remarks>
internal static class ClientSettingsBuilder
{
    /// <summary>
    /// Builds <see cref="MongoClientSettings"/> from the given connection string and options.
    /// </summary>
    /// <param name="connectionString">The MongoDB connection URI.</param>
    /// <param name="options">The provider options to apply.</param>
    /// <returns>Configured <see cref="MongoClientSettings"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectionString"/> is null or empty.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="connectionString"/> is not a valid MongoDB URI.</exception>
    public static MongoClientSettings Build(string connectionString, MongoDbOptions options)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentNullException.ThrowIfNull(options);

        MongoClientSettings settings;
        try
        {
            settings = MongoClientSettings.FromConnectionString(connectionString);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException(
                $"The connection string is not a valid MongoDB URI: {ex.Message}", nameof(connectionString), ex);
        }

        // Pool settings
        settings.MaxConnectionPoolSize = options.MaxConnectionPoolSize;
        settings.MinConnectionPoolSize = options.MinConnectionPoolSize;

        // Timeouts
        settings.ConnectTimeout = TimeSpan.FromSeconds(options.ConnectTimeoutSeconds);
        settings.ServerSelectionTimeout = TimeSpan.FromSeconds(options.ServerSelectionTimeoutSeconds);
        settings.SocketTimeout = TimeSpan.FromSeconds(options.SocketTimeoutSeconds);

        // Connection lifetime
        if (options.MaxConnectionLifeTimeSeconds > 0)
        {
            settings.MaxConnectionIdleTime = TimeSpan.FromSeconds(options.MaxConnectionLifeTimeSeconds);
        }

        // Retry
        settings.RetryWrites = options.RetryWrites;
        settings.RetryReads = options.RetryReads;

        // Read preference
        if (options.ReadPreference is not null)
        {
            settings.ReadPreference = options.ReadPreference switch
            {
                "Primary" => ReadPreference.Primary,
                "PrimaryPreferred" => ReadPreference.PrimaryPreferred,
                "Secondary" => ReadPreference.Secondary,
                "SecondaryPreferred" => ReadPreference.SecondaryPreferred,
                "Nearest" => ReadPreference.Nearest,
                _ => ReadPreference.Primary
            };
        }

        // Write concern
        if (options.WriteConcern is not null)
        {
            settings.WriteConcern = options.WriteConcern switch
            {
                "Acknowledged" => WriteConcern.Acknowledged,
                "Unacknowledged" => WriteConcern.Unacknowledged,
                "W1" => new WriteConcern(1),
                "W2" => new WriteConcern(2),
                "W3" => new WriteConcern(3),
                "Majority" => WriteConcern.WMajority,
                var w when w.StartsWith("W", StringComparison.OrdinalIgnoreCase) && int.TryParse(w[1..], out var wValue) => new WriteConcern(wValue),
                _ => WriteConcern.Acknowledged
            };
        }

        // Custom settings delegate
        options.ConfigureClientSettings?.Invoke(settings);

        return settings;
    }
}
