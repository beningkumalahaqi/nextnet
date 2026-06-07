using NextNet.Cli.Errors;

namespace NextNet.Cli.Config;

/// <summary>
/// Validates a <see cref="NextNetProjectConfig"/> instance against
/// NextNet's schema rules and returns validation warnings/errors.
/// </summary>
public static class ConfigValidator
{
    /// <summary>
    /// Validate the configuration and return a list of issues found.
    /// Returns an empty list if the configuration is valid.
    /// </summary>
    /// <param name="config">The configuration to validate.</param>
    /// <returns>A list of validation issues (errors and warnings).</returns>
    public static List<ConfigIssue> Validate(NextNetProjectConfig? config)
    {
        var issues = new List<ConfigIssue>();

        if (config is null)
        {
            issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, ErrorCodes.ConfigFileNotFound.Code,
                "Configuration is null"));
            return issues;
        }

        // Validate name
        if (string.IsNullOrWhiteSpace(config.Name))
        {
            issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, ErrorCodes.InvalidProjectName.Code,
                "Project name is required in configuration"));
        }
        else if (!ProjectNameRegex.IsValid(config.Name))
        {
            issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, ErrorCodes.InvalidProjectName.Code,
                $"Project name '{config.Name}' is invalid. Use kebab-case (e.g., 'my-app')."));
        }

        // Validate version format (semver-like)
        if (!string.IsNullOrWhiteSpace(config.Version) && !System.Text.RegularExpressions.Regex.IsMatch(config.Version, @"^\d+\.\d+\.\d+"))
        {
            issues.Add(new ConfigIssue(ConfigIssueSeverity.Warning, "NN-006",
                $"Version '{config.Version}' does not follow semver format (expected: x.y.z)"));
        }

        // Validate routing dir
        if (config.Routing is not null)
        {
            if (string.IsNullOrWhiteSpace(config.Routing.Dir))
            {
                issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, "NN-006",
                    "Routing directory cannot be empty"));
            }
            else if (config.Routing.Dir.Contains('/') || config.Routing.Dir.Contains('\\'))
            {
                issues.Add(new ConfigIssue(ConfigIssueSeverity.Warning, "NN-006",
                    $"Routing directory '{config.Routing.Dir}' should be a simple directory name, not a path"));
            }
        }

        // Validate build config
        if (config.Build is not null)
        {
            if (string.IsNullOrWhiteSpace(config.Build.Output))
            {
                issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, "NN-006",
                    "Build output directory cannot be empty"));
            }

            if (!string.IsNullOrWhiteSpace(config.Build.Target) &&
                !config.Build.Target.StartsWith("net"))
            {
                issues.Add(new ConfigIssue(ConfigIssueSeverity.Warning, "NN-006",
                    $"Build target '{config.Build.Target}' does not look like a valid .NET target framework"));
            }
        }

        // Validate dev config
        if (config.Dev is not null)
        {
            if (config.Dev.Port < 1 || config.Dev.Port > 65535)
            {
                issues.Add(new ConfigIssue(ConfigIssueSeverity.Error, "NN-006",
                    $"Dev port {config.Dev.Port} is invalid. Must be between 1 and 65535."));
            }
        }

        return issues;
    }

    /// <summary>
    /// Validate and throw if any errors are found.
    /// </summary>
    public static void ValidateOrThrow(NextNetProjectConfig? config)
    {
        var issues = Validate(config);
        var errors = issues.Where(i => i.Severity == ConfigIssueSeverity.Error).ToList();

        if (errors.Count > 0)
        {
            var messages = string.Join("; ", errors.Select(e => e.Message));
            throw new NextNetConfigException(ErrorCodes.InvalidConfigFile, messages);
        }
    }
}

/// <summary>
/// Represents a single configuration validation issue.
/// </summary>
/// <param name="Severity">Whether this is an error or warning.</param>
/// <param name="Code">The associated error code.</param>
/// <param name="Message">Human-readable description of the issue.</param>
public sealed record ConfigIssue(ConfigIssueSeverity Severity, string Code, string Message);

/// <summary>Severity level for configuration issues.</summary>
public enum ConfigIssueSeverity
{
    /// <summary>Must be fixed for the configuration to be valid.</summary>
    Error,
    /// <summary>Should be reviewed but doesn't prevent operation.</summary>
    Warning
}

/// <summary>Validates project names match kebab-case pattern.</summary>
public static class ProjectNameRegex
{
    private static readonly System.Text.RegularExpressions.Regex Pattern =
        new(@"^[a-z][a-z0-9-]*$", System.Text.RegularExpressions.RegexOptions.Compiled);

    /// <summary>Returns true if the name is a valid kebab-case project name.</summary>
    public static bool IsValid(string name) => Pattern.IsMatch(name);
}

/// <summary>
/// Exception thrown when a configuration error is encountered.
/// </summary>
public sealed class NextNetConfigException : Exception
{
    /// <summary>The associated error entry.</summary>
    public ErrorEntry ErrorEntry { get; }

    /// <summary>
    /// Creates a new configuration exception.
    /// </summary>
    public NextNetConfigException(ErrorEntry errorEntry, string message)
        : base(message)
    {
        ErrorEntry = errorEntry;
    }
}
