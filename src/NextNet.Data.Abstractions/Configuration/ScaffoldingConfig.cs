using System.Text.Json.Serialization;

namespace NextNet.Data.Abstractions.Configuration;

/// <summary>
/// Configuration options for scaffolding and code generation.
/// </summary>
/// <remarks>
/// <para>
/// Scaffolding settings control how model classes, repository classes, and CRUD operations
/// are generated from database schemas. These options are consumed by the
/// <see cref="Abstractions.IScaffoldProvider"/> implementation.
/// </para>
/// <example>
/// <code>
/// var config = new ScaffoldingConfig(
///     modelsNamespace: "App.Domain.Models",
///     repositoriesNamespace: "App.Data.Repositories",
///     modelsDirectory: "Domain/Models",
///     repositoriesDirectory: "Data/Repositories",
///     overwriteExisting: true);
/// </code>
/// </example>
/// </remarks>
/// <param name="ModelsNamespace">The namespace for generated model classes. Defaults to <c>"Models"</c>.</param>
/// <param name="RepositoriesNamespace">The namespace for generated repository classes. Defaults to <c>"Repositories"</c>.</param>
/// <param name="ActionsNamespace">The namespace for generated action classes. Defaults to <c>"Actions"</c>.</param>
/// <param name="ModelsDirectory">The output directory for generated models, relative to project root. Defaults to <c>"Models"</c>.</param>
/// <param name="RepositoriesDirectory">The output directory for generated repositories, relative to project root. Defaults to <c>"Repositories"</c>.</param>
/// <param name="ActionsDirectory">The output directory for generated action files, relative to project root. Defaults to <c>"app/api"</c>.</param>
/// <param name="OverwriteExisting">Whether to overwrite existing files when generating. Defaults to <c>false</c>.</param>
public sealed record ScaffoldingConfig(
    [property: JsonPropertyName("modelsNamespace")]
    string ModelsNamespace = "Models",

    [property: JsonPropertyName("repositoriesNamespace")]
    string RepositoriesNamespace = "Repositories",

    [property: JsonPropertyName("actionsNamespace")]
    string ActionsNamespace = "Actions",

    [property: JsonPropertyName("modelsDirectory")]
    string ModelsDirectory = "Models",

    [property: JsonPropertyName("repositoriesDirectory")]
    string RepositoriesDirectory = "Repositories",

    [property: JsonPropertyName("actionsDirectory")]
    string ActionsDirectory = "app/api",

    [property: JsonPropertyName("overwriteExisting")]
    bool OverwriteExisting = false
);
