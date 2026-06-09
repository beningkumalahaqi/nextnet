using NextNet.Configuration;

namespace NextNet.Isr.Configuration;

/// <summary>
/// Validates the ISR configuration model for consistency and correctness.
/// Ensures that options, route metadata, and global settings are valid
/// before the ISR system is initialized.
/// </summary>
public static class IsrConfigValidator
{
    /// <summary>
    /// Validates all ISR configuration and returns a list of errors.
    /// </summary>
    /// <param name="globalOptions">The global ISR options.</param>
    /// <param name="manifest">The ISR manifest with per-route metadata.</param>
    /// <returns>A list of configuration errors. Empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public static IReadOnlyList<ConfigError> Validate(IsrGlobalOptions globalOptions, Manifest.IsrManifest manifest)
    {
        if (globalOptions == null) throw new ArgumentNullException(nameof(globalOptions));
        if (manifest == null) throw new ArgumentNullException(nameof(manifest));

        var errors = new List<ConfigError>();

        // Validate global options
        if (globalOptions.DefaultRevalidateSeconds < 0)
            errors.Add(new ConfigError("ISR_CFG_001",
                "DefaultRevalidateSeconds must be non-negative.",
                ConfigErrorSeverity.Error,
                Path: "IsrGlobalOptions.DefaultRevalidateSeconds"));

        if (globalOptions.MaxConcurrentRegenerations < 1)
            errors.Add(new ConfigError("ISR_CFG_002",
                "MaxConcurrentRegenerations must be at least 1.",
                ConfigErrorSeverity.Error,
                Path: "IsrGlobalOptions.MaxConcurrentRegenerations"));

        if (globalOptions.MaxPendingRevalidations < 1)
            errors.Add(new ConfigError("ISR_CFG_003",
                "MaxPendingRevalidations must be at least 1.",
                ConfigErrorSeverity.Error,
                Path: "IsrGlobalOptions.MaxPendingRevalidations"));

        if (globalOptions.DeduplicationWindowSeconds < 0)
            errors.Add(new ConfigError("ISR_CFG_004",
                "DeduplicationWindowSeconds must be non-negative.",
                ConfigErrorSeverity.Error,
                Path: "IsrGlobalOptions.DeduplicationWindowSeconds"));

        // Validate per-route metadata
        foreach (var kvp in manifest.Routes)
        {
            var route = kvp.Key;
            var metadata = kvp.Value;

            if (metadata.RevalidateSeconds < 0)
                errors.Add(new ConfigError("ISR_CFG_005",
                    "RevalidateSeconds must be non-negative.",
                    ConfigErrorSeverity.Error,
                    Path: $"IsrRouteMetadata[{route}].RevalidateSeconds"));

            if (metadata.MaxConcurrentRegenerations < 1)
                errors.Add(new ConfigError("ISR_CFG_006",
                    "MaxConcurrentRegenerations must be at least 1.",
                    ConfigErrorSeverity.Error,
                    Path: $"IsrRouteMetadata[{route}].MaxConcurrentRegenerations"));
        }

        return errors;
    }
}
