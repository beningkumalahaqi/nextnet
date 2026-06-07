using System.Text.RegularExpressions;

namespace NextNet.Cli.Interactive;

/// <summary>
/// Defines the metadata for a single user prompt, including its name, description,
/// default value, allowed choices, and validation logic.
/// </summary>
/// <remarks>
/// <para>
/// A <see cref="PromptDefinition"/> describes how a prompt should be presented to
/// the user and how its input should be validated. It does not contain user state
/// or the actual value entered by the user.
/// </para>
/// <para>
/// When <see cref="AllowedValues"/> is non-null, the prompt is typically rendered
/// as a numbered choice list. Otherwise it is rendered as a free-form text input.
/// </para>
/// </remarks>
public sealed record PromptDefinition
{
    /// <summary>
    /// Gets the unique variable name this prompt corresponds to (e.g., "projectName", "database").
    /// </summary>
    public string Name { get; init; } = "";

    /// <summary>
    /// Gets the human-readable description shown to the user.
    /// </summary>
    public string Description { get; init; } = "";

    /// <summary>
    /// Gets the default value used when the user provides no input.
    /// </summary>
    public string? DefaultValue { get; init; }

    /// <summary>
    /// Gets the list of allowed values for choice-based prompts.
    /// When non-null and non-empty, the prompt is rendered as a numbered selection.
    /// </summary>
    public IReadOnlyList<string>? AllowedValues { get; init; }

    /// <summary>
    /// Gets an optional validation function that returns an error message if the value is invalid,
    /// or <c>null</c> if the value is valid.
    /// </summary>
    public Func<string, string?>? Validator { get; init; }

    /// <summary>
    /// Gets a value indicating whether the user must provide a non-empty value.
    /// </summary>
    public bool Required { get; init; }
}

/// <summary>
/// Provides factory methods for creating common <see cref="PromptDefinition"/> instances
/// used during interactive project generation.
/// </summary>
/// <remarks>
/// <para>
/// Each static method returns a predefined <see cref="PromptDefinition"/> with sensible
/// defaults, validation rules, and allowed values where applicable. Use these in
/// conjunction with <see cref="ProjectPrompts"/> to present prompts to the user.
/// </para>
/// </remarks>
public static class PromptDefinitions
{
    /// <summary>
    /// Creates a prompt definition for the project name.
    /// Validates that the name is non-empty, starts with a letter, and contains only
    /// letters, numbers, underscores, and hyphens.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for project name input.</returns>
    public static PromptDefinition ProjectName() => new()
    {
        Name = "projectName",
        Description = "Project name (e.g., 'MyApp')",
        Required = true,
        Validator = name =>
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Project name cannot be empty";
            if (!Regex.IsMatch(name, "^[a-zA-Z][a-zA-Z0-9_-]*$"))
                return "Project name must start with a letter and contain only letters, numbers, underscores, and hyphens";
            return null;
        }
    };

    /// <summary>
    /// Creates a prompt definition for database provider selection.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for database choice.</returns>
    public static PromptDefinition Database() => new()
    {
        Name = "database",
        Description = "Database provider",
        DefaultValue = "sqlite",
        AllowedValues = new[] { "sqlite", "postgres", "none" }
    };

    /// <summary>
    /// Creates a prompt definition for authentication scaffold inclusion.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for auth yes/no.</returns>
    public static PromptDefinition Authentication() => new()
    {
        Name = "includeAuth",
        Description = "Include authentication scaffold?",
        DefaultValue = "false",
        AllowedValues = new[] { "true", "false" }
    };

    /// <summary>
    /// Creates a prompt definition for server-side rendering option.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for SSR choice.</returns>
    public static PromptDefinition Ssr() => new()
    {
        Name = "useSsr",
        Description = "Use server-side rendering?",
        DefaultValue = "true",
        AllowedValues = new[] { "true", "false" }
    };

    /// <summary>
    /// Creates a prompt definition for static site generation option.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for SSG choice.</returns>
    public static PromptDefinition Ssg() => new()
    {
        Name = "useSsg",
        Description = "Use static site generation?",
        DefaultValue = "false",
        AllowedValues = new[] { "true", "false" }
    };

    /// <summary>
    /// Creates a prompt definition for the base URL of the project.
    /// Validates that the URL is a valid absolute URI.
    /// </summary>
    /// <returns>A configured <see cref="PromptDefinition"/> for base URL input.</returns>
    public static PromptDefinition BaseUrl() => new()
    {
        Name = "baseUrl",
        Description = "Base URL for the project",
        DefaultValue = "http://localhost:5000",
        Validator = url =>
        {
            if (string.IsNullOrWhiteSpace(url))
                return "Base URL cannot be empty";
            if (!Uri.TryCreate(url, UriKind.Absolute, out _))
                return "Base URL must be a valid URL (e.g., http://localhost:5000)";
            return null;
        }
    };
}
