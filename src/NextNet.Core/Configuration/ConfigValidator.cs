using NextNet.Errors;

namespace NextNet.Configuration;

/// <summary>
/// Validates <see cref="NextNetConfig"/> instances and returns a list of
/// configuration errors and warnings.
/// </summary>
public static class ConfigValidator
{
    /// <summary>
    /// Validates the specified <paramref name="config"/> and returns any issues found.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <returns>A read-only list of validation errors and warnings.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is <c>null</c>.</exception>
    public static IReadOnlyList<ConfigError> Validate(NextNetConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        var errors = new List<ConfigError>();

        // AppDir must not be null or empty
        if (string.IsNullOrWhiteSpace(config.AppDir))
        {
            errors.Add(new ConfigError(
                CoreErrorCodes.ConfigAppDirEmpty,
                "Application directory (AppDir) must not be null or empty.",
                ConfigErrorSeverity.Error,
                nameof(config.AppDir)));
        }

        // DevPort must be between 1 and 65535
        if (config.DevPort < 1 || config.DevPort > 65535)
        {
            errors.Add(new ConfigError(
                CoreErrorCodes.ConfigDevPortOutOfRange,
                $"Development port ({config.DevPort}) must be between 1 and 65535.",
                ConfigErrorSeverity.Error,
                nameof(config.DevPort)));
        }

        // WatchDebounceMs must be greater than 0
        if (config.WatchDebounceMs <= 0)
        {
            errors.Add(new ConfigError(
                CoreErrorCodes.ConfigWatchDebounceInvalid,
                $"Watch debounce milliseconds ({config.WatchDebounceMs}) must be greater than 0.",
                ConfigErrorSeverity.Error,
                nameof(config.WatchDebounceMs)));
        }

        return errors.AsReadOnly();
    }
}
