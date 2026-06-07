namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Options for controlling scaffold code generation output.
/// Provides defaults from <see cref="Configuration.ScaffoldingConfig"/> with per-command overrides.
/// </summary>
/// <param name="OutputDirectory">The root output directory for generated files. Defaults to the project root.</param>
/// <param name="ModelsDirectory">Subdirectory for models. Defaults to "Models".</param>
/// <param name="RepositoriesDirectory">Subdirectory for repositories. Defaults to "Repositories".</param>
/// <param name="ActionsDirectory">Subdirectory for server action files. Defaults to "app/api".</param>
/// <param name="ModelsNamespace">Namespace for generated model classes. Defaults to "{Project}.Models".</param>
/// <param name="RepositoriesNamespace">Namespace for generated repository classes. Defaults to "{Project}.Repositories".</param>
/// <param name="ActionsNamespace">Namespace for generated action classes. Defaults to "{Project}.Actions".</param>
/// <param name="OverwriteExisting">Whether to overwrite existing files. Defaults to false.</param>
/// <param name="DryRun">If true, only return artifact metadata without writing files. Defaults to false.</param>
/// <param name="ProjectNamespace">The project's root namespace. Inferred from nextnet.config.json or project name.</param>
/// <param name="Properties">Optional list of properties to include in the generated model. If empty, generates a simple entity with Id.</param>
public sealed record ScaffoldOptions(
    string OutputDirectory = ".",
    string ModelsDirectory = "Models",
    string RepositoriesDirectory = "Repositories",
    string ActionsDirectory = "app/api",
    string ModelsNamespace = "{Project}.Models",
    string RepositoriesNamespace = "{Project}.Repositories",
    string ActionsNamespace = "{Project}.Actions",
    bool OverwriteExisting = false,
    bool DryRun = false,
    string? ProjectNamespace = null,
    IReadOnlyList<ScaffoldProperty>? Properties = null
);

/// <summary>
/// Describes a single property to be included in a generated model class.
/// </summary>
/// <param name="Name">The property name (e.g., "FirstName").</param>
/// <param name="Type">The C# type name (e.g., "string", "int", "DateTime"). Defaults to "string".</param>
/// <param name="IsRequired">Whether the property is required (non-nullable). Defaults to false.</param>
/// <param name="MaxLength">Optional maximum length for string properties.</param>
/// <param name="IsKey">Whether this property is the primary key. Only one key supported; defaults to "Id" auto-generated.</param>
public sealed record ScaffoldProperty(
    string Name,
    string Type = "string",
    bool IsRequired = false,
    int? MaxLength = null,
    bool IsKey = false
);
