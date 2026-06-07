namespace NextNet.Templates.Official.Api;

using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

/// <summary>
/// Template provider for the official REST API template.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ApiTemplateProvider"/> implements <see cref="ITemplateProvider"/>
/// to deliver the official REST API template. The template generates a production-ready
/// ASP.NET Core Web API with OpenAPI/Swagger documentation, health checks, and
/// optional JWT authentication scaffolding.
/// </para>
/// <para>
/// This provider responds only to the template name <c>"api"</c>. Any other template
/// name throws <see cref="TemplateNotFoundException"/>.
/// </para>
/// <example>
/// <code>
/// var provider = new ApiTemplateProvider();
/// var manifest = await provider.GetManifestAsync("api");
/// var files = await provider.GetFilesAsync(manifest);
/// </code>
/// </example>
/// </remarks>
public sealed class ApiTemplateProvider : ITemplateProvider
{
    /// <summary>
    /// Gets the unique provider name: <c>"api-official"</c>.
    /// </summary>
    public string Name => "api-official";

    /// <summary>
    /// Retrieves the parsed <see cref="TemplateManifest"/> for the specified template.
    /// </summary>
    /// <param name="templateName">The name of the template to resolve.</param>
    /// <param name="version">An optional semantic version constraint. If <c>null</c>, the latest version is returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parsed template manifest.</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when <paramref name="templateName"/> is not <c>"api"</c>.</exception>
    public Task<TemplateManifest> GetManifestAsync(
        string templateName,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        if (templateName != "api")
            throw new TemplateNotFoundException(templateName, version);

        return Task.FromResult(ApiTemplateManifest.Create());
    }

    /// <summary>
    /// Loads the file contents for a given manifest into memory as a dictionary of
    /// source paths to byte arrays.
    /// </summary>
    /// <param name="manifest">The manifest whose files should be loaded.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A dictionary mapping source paths to raw file content bytes.</returns>
    public Task<IReadOnlyDictionary<string, byte[]>> GetFilesAsync(
        TemplateManifest manifest,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ApiTemplateFiles.GetAllFiles());
    }

    /// <summary>
    /// Checks whether a template with the given name exists in this provider.
    /// </summary>
    /// <param name="templateName">The name of the template to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if <paramref name="templateName"/> is <c>"api"</c>; otherwise <c>false</c>.</returns>
    public Task<bool> ExistsAsync(string templateName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(templateName == "api");
    }
}
