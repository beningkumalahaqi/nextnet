using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Defines the contract for a template registry that provides search, discovery, and
/// version management across multiple template providers.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ITemplateRegistry"/> aggregates templates from one or more
/// <see cref="ITemplateProvider"/> instances and provides a unified API for searching
/// templates, retrieving manifests, and listing available versions.
/// </para>
/// <para>
/// Unlike <see cref="ITemplateProvider"/> which is source-specific, the registry
/// is a cross-cutting service that users interact with when browsing or selecting
/// templates from the CLI or IDE tooling.
/// </para>
/// <example>
/// <code>
/// var registry = new TemplateRegistry(providers);
/// var results = await registry.SearchAsync("webapi");
///
/// foreach (var manifest in results)
///     Console.WriteLine($"{manifest.Name} v{manifest.Version}");
/// </code>
/// </example>
/// </remarks>
public interface ITemplateRegistry
{
    /// <summary>
    /// Searches for templates matching the given query string across all registered providers.
    /// </summary>
    /// <param name="query">The search query to match against template names, tags, and descriptions.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of matching template manifests.</returns>
    Task<IReadOnlyList<TemplateManifest>> SearchAsync(string query, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the manifest for a specific template by name, optionally at a specific version.
    /// </summary>
    /// <param name="templateName">The name of the template to retrieve.</param>
    /// <param name="version">An optional semantic version constraint. If <c>null</c>, the latest version is returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parsed template manifest.</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when the template cannot be found.</exception>
    Task<TemplateManifest> GetManifestAsync(string templateName, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all available versions of a given template.
    /// </summary>
    /// <param name="templateName">The name of the template whose versions to list.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of semantic version strings sorted in descending order (newest first).</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when the template cannot be found.</exception>
    Task<IReadOnlyList<string>> GetVersionsAsync(string templateName, CancellationToken cancellationToken = default);
}
