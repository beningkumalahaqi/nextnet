using System.Linq;
using NextNet.Core.Extensions;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Models;
using NextNet.TemplateEngine.Variables;

namespace NextNet.Cli.Interactive;

/// <summary>
/// Orchestrates the interactive project generation flow, guiding users through
/// template selection, project configuration, and variable prompting.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="InteractiveProjectGenerator"/> discovers available templates from
/// registered <see cref="ITemplateProvider"/> instances and presents the user with
/// a series of prompts to configure the project. Common prompts include project name,
/// template selection, database provider, authentication, and base URL.
/// </para>
/// <para>
/// When an <see cref="InteractiveOptions"/> instance is provided with values pre-set,
/// those values are used directly without prompting, enabling non-interactive usage
/// and testability.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var providers = new[] { new BlogTemplateProvider(), new ApiTemplateProvider() };
/// var gen = new InteractiveProjectGenerator(providers);
///
/// // Interactive mode (prompts user)
/// var result = await gen.GenerateAsync();
///
/// // Non-interactive mode (pre-configured options)
/// var result = await gen.GenerateAsync(new InteractiveOptions
/// {
///     ProjectName = "MyBlog",
///     TemplateName = "blog",
///     AuthorName = "Jane"
/// });
/// </code>
/// </example>
public sealed class InteractiveProjectGenerator
{
    private readonly IReadOnlyList<ITemplateProvider> _providers;

    /// <summary>
    /// Initializes a new instance of the <see cref="InteractiveProjectGenerator"/> class.
    /// </summary>
    /// <param name="providers">The template providers to discover available templates from.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providers"/> is null.</exception>
    public InteractiveProjectGenerator(IEnumerable<ITemplateProvider> providers)
    {
        _providers = providers?.ToList() ?? throw new ArgumentNullException(nameof(providers));
    }

    /// <summary>
    /// Runs the interactive project generation flow, prompting the user for configuration
    /// values and returning the collected result.
    /// </summary>
    /// <param name="options">Optional pre-configured values to skip prompts for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An <see cref="InteractiveGenerationResult"/> containing the selected template,
    /// variable context, and manifest.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the specified template is not found
    /// or when required providers are missing.</exception>
    public async Task<InteractiveGenerationResult> GenerateAsync(
        InteractiveOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new InteractiveOptions();

        Console.WriteLine();
        Console.WriteLine("\u2554\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2557");
        Console.WriteLine("\u2551   NextNet Project Generator          \u2551");
        Console.WriteLine("\u255a\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u2550\u255d");
        Console.WriteLine();

        var builder = new InteractiveVariableContextBuilder();

        // 1. Project name
        string projectName;
        if (string.IsNullOrEmpty(options.ProjectName))
        {
            projectName = ProjectPrompts.PromptProjectName(PromptDefinitions.ProjectName());
            builder.SetString("projectName", projectName);
        }
        else
        {
            projectName = options.ProjectName;
            builder.SetString("projectName", projectName);
        }

        builder.SetString("namespaceName", StringCaseHelper.ToPascalCase(projectName));

        // 2. Template selection
        string templateName;
        if (!string.IsNullOrEmpty(options.TemplateName))
        {
            templateName = options.TemplateName;
        }
        else
        {
            templateName = await PromptTemplateAsync(cancellationToken);
        }

        // Load template to discover its variables
        var provider = FindProvider(templateName);
        if (provider is null)
        {
            throw new InvalidOperationException($"Template '{templateName}' not found.");
        }

        var manifest = await provider.GetManifestAsync(templateName, cancellationToken: cancellationToken);
        builder.SetString("templateName", templateName);

        // 3. Database
        if (HasVariable(manifest, "database"))
        {
            var dbValue = options.Database ?? ProjectPrompts.PromptChoice(PromptDefinitions.Database());
            builder.SetString("database", dbValue);
        }

        // 4. Authentication
        if (HasVariable(manifest, "includeAuth"))
        {
            var authValue = options.IncludeAuth ?? ProjectPrompts.PromptYesNo(PromptDefinitions.Authentication());
            builder.SetBool("includeAuth", authValue);
        }

        // 5. Author name
        if (HasVariable(manifest, "authorName"))
        {
            var author = options.AuthorName ?? PromptString("Author name", "Anonymous");
            builder.SetString("authorName", author);
        }

        // 6. Base URL
        if (HasVariable(manifest, "baseUrl"))
        {
            var url = options.BaseUrl ?? ProjectPrompts.PromptProjectName(PromptDefinitions.BaseUrl());
            builder.SetString("baseUrl", url);
        }

        return new InteractiveGenerationResult
        {
            TemplateName = templateName,
            VariableContext = builder.Build(),
            Manifest = manifest
        };
    }

    /// <summary>
    /// Prompts the user to select a template from available providers.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The selected template name.</returns>
    private async Task<string> PromptTemplateAsync(CancellationToken ct)
    {
        Console.WriteLine("Available templates:");
        var templateNames = new List<string>();
        foreach (var provider in _providers)
        {
            // Try common template names
            foreach (var name in new[] { "blog", "api", "dashboard", "saas" })
            {
                if (!templateNames.Contains(name) && await provider.ExistsAsync(name, ct))
                {
                    templateNames.Add(name);
                    Console.WriteLine($"  {templateNames.Count}. {name}");
                }
            }
        }

        if (templateNames.Count == 0)
        {
            Console.WriteLine("  (no templates found)");
            throw new InvalidOperationException("No templates are available. Ensure template providers are registered.");
        }

        Console.Write($"Select template [1]: ");
        var input = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(input)) return templateNames[0];
        if (int.TryParse(input, out var idx) && idx >= 1 && idx <= templateNames.Count)
        {
            return templateNames[idx - 1];
        }
        return input;
    }

    /// <summary>
    /// Finds the <see cref="ITemplateProvider"/> that can serve the specified template name.
    /// </summary>
    /// <param name="templateName">The template name to locate.</param>
    /// <returns>The matching provider, or <c>null</c> if none was found.</returns>
    private ITemplateProvider? FindProvider(string templateName)
    {
        foreach (var provider in _providers)
        {
            var name = provider.Name.Replace("-official", "", StringComparison.OrdinalIgnoreCase);
            if (string.Equals(name, templateName, StringComparison.OrdinalIgnoreCase))
                return provider;
        }

        // Fall back to ExistsAsync check for custom-named templates
        foreach (var provider in _providers)
        {
            if (provider.ExistsAsync(templateName).GetAwaiter().GetResult())
                return provider;
        }

        return null;
    }

    /// <summary>
    /// Determines whether the template manifest declares a variable with the specified name.
    /// </summary>
    /// <param name="manifest">The template manifest to inspect.</param>
    /// <param name="name">The variable name to check for.</param>
    /// <returns><c>true</c> if the manifest declares the variable; otherwise <c>false</c>.</returns>
    private static bool HasVariable(TemplateManifest manifest, string name)
    {
        return manifest.Variables?.Any(v => v.Name == name) == true;
    }

    /// <summary>
    /// Prompts the user for a free-form string value with a default.
    /// </summary>
    /// <param name="description">The prompt description.</param>
    /// <param name="defaultValue">The default value used if the user enters nothing.</param>
    /// <returns>The entered or default value.</returns>
    private static string PromptString(string description, string defaultValue)
    {
        Console.Write($"{description} [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}

/// <summary>
/// Configuration options for the interactive project generator.
/// When a property is set, the corresponding prompt is skipped and the value is used directly.
/// </summary>
/// <remarks>
/// <para>
/// Set <see cref="NonInteractive"/> to <c>true</c> when all required values are provided,
/// suppressing all console prompts.
/// </para>
/// </remarks>
public sealed record InteractiveOptions
{
    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    public string? ProjectName { get; init; }

    /// <summary>
    /// Gets or sets the template name (e.g., "blog", "api").
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// Gets or sets the database provider (e.g., "sqlite", "postgres", "none").
    /// </summary>
    public string? Database { get; init; }

    /// <summary>
    /// Gets or sets whether to include authentication scaffold.
    /// </summary>
    public bool? IncludeAuth { get; init; }

    /// <summary>
    /// Gets or sets the author name.
    /// </summary>
    public string? AuthorName { get; init; }

    /// <summary>
    /// Gets or sets the base URL (e.g., "http://localhost:5000").
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether to run in non-interactive mode.
    /// When <c>true</c>, no console prompts are shown.
    /// </summary>
    public bool NonInteractive { get; init; }
}

/// <summary>
/// Represents the result of an interactive project generation session.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="InteractiveGenerationResult"/> contains all the information needed
/// by the <c>NewCommand</c> to invoke the template engine and generate the project files.
/// </para>
/// </remarks>
public sealed record InteractiveGenerationResult
{
    /// <summary>
    /// Gets the name of the selected template.
    /// </summary>
    public string TemplateName { get; init; } = "";

    /// <summary>
    /// Gets the variable context containing all user-provided configuration values.
    /// </summary>
    public VariableContext VariableContext { get; init; } = null!;

    /// <summary>
    /// Gets the template manifest for the selected template.
    /// </summary>
    public TemplateManifest Manifest { get; init; } = null!;
}
