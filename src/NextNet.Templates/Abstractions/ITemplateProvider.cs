using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

namespace NextNet.Templates.Abstractions;

/// <summary>
/// Defines the contract for template providers that discover, resolve, and deliver
/// template packages from various sources (file system, NuGet, HTTP endpoints, etc.).
/// </summary>
/// <remarks>
/// <para>
/// Implementations of <see cref="ITemplateProvider"/> are responsible for locating
/// templates by name, reading their manifests, and loading their file contents into
/// memory. Each provider has a unique <see cref="Name"/> that identifies its source
/// type (e.g., "filesystem", "nuget", "http").
/// </para>
/// <para>
/// The provider does NOT perform template rendering or validation — it only handles
/// template discovery and content retrieval. Rendering is handled by
/// <see cref="ITemplateEngine"/>.
/// </para>
/// <example>
/// <code>
/// public class FileSystemTemplateProvider : ITemplateProvider
/// {
///     public string Name => "filesystem";
///
///     public async Task&lt;TemplateManifest&gt; GetManifestAsync(
///         string templateName,
///         string? version = null,
///         CancellationToken ct = default)
///     {
///         var path = Path.Combine(_root, templateName, "template.json");
///         var json = await File.ReadAllTextAsync(path, ct);
///         return JsonSerializer.Deserialize&lt;TemplateManifest&gt;(json)!;
///     }
/// }
/// </code>
/// </example>
/// </remarks>
public interface ITemplateProvider
{
    /// <summary>
    /// Gets a unique name that identifies this provider (e.g., "filesystem", "nuget").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves the parsed <see cref="TemplateManifest"/> for the specified template.
    /// </summary>
    /// <param name="templateName">The name of the template to resolve.</param>
    /// <param name="version">An optional semantic version constraint. If <c>null</c>, the latest version is returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parsed template manifest.</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when the template cannot be found.</exception>
    Task<TemplateManifest> GetManifestAsync(string templateName, string? version = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the file contents for a given manifest into memory as a dictionary of
    /// source paths to byte arrays.
    /// </summary>
    /// <param name="manifest">The manifest whose files should be loaded.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary mapping source paths to raw file content bytes.</returns>
    Task<IReadOnlyDictionary<string, byte[]>> GetFilesAsync(TemplateManifest manifest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a template with the given name exists in this provider.
    /// </summary>
    /// <param name="templateName">The name of the template to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if the template exists; otherwise <c>false</c>.</returns>
    Task<bool> ExistsAsync(string templateName, CancellationToken cancellationToken = default);
}
