using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Defines the contract for provider-aware code generation (scaffolding).
/// Each data provider (EF Core, Dapper, MongoDB) implements this to generate
/// idiomatic code for its persistence technology.
/// </summary>
/// <remarks>
/// <para>
/// The scaffold provider generates three types of artifacts:
/// <list type="bullet">
///   <item><description><b>Model</b> — A POCO class representing the entity.</description></item>
///   <item><description><b>Repository</b> — A CRUD repository class using the provider's API.</description></item>
///   <item><description><b>CRUD Actions</b> — Server action files for Create, Read, Update, Delete operations.</description></item>
/// </list>
/// </para>
/// <para>
/// Template-based generation uses the <c>TemplateEngine</c> with {{PLACEHOLDER}} replacement.
/// Provider-specific templates are embedded as resources in each provider assembly.
/// </para>
/// </remarks>
public interface IScaffoldProvider
{
    /// <summary>
    /// Gets the name of this scaffold provider (e.g., "EntityFramework", "Dapper", "MongoDB").
    /// Matches the provider name used in configuration.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Generates a model class file for the specified entity.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a repository class file for the specified entity.
    /// The repository implements <see cref="IRepository{T}"/> using provider-specific APIs.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="ScaffoldArtifact"/> describing the generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a full set of CRUD server action files for the specified entity.
    /// Produces separate files for Create, Read (list + detail), Update, and Delete operations.
    /// </summary>
    /// <param name="entityName">The name of the entity (e.g., "User", "Product").</param>
    /// <param name="options">Options controlling output paths, namespace, and overwrite behavior.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of <see cref="ScaffoldArtifact"/> describing each generated file.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="entityName"/> is null or empty.</exception>
    Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default);
}
