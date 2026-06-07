namespace NextNet.Templates.Official.Saas;

using NextNet.Templates.Abstractions;
using NextNet.Templates.Exceptions;
using NextNet.Templates.Models;

/// <summary>
/// Template provider for the official SaaS template.
/// Loads template manifest and file content from static definitions.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="SaasTemplateProvider"/> implements <see cref="ITemplateProvider"/>
/// to deliver the official SaaS template. The template generates a multi-tenant SaaS
/// starter application with users, organizations, authentication, and optional billing.
/// </para>
/// <para>
/// This provider responds only to the template name <c>"saas"</c>. Any other template
/// name throws <see cref="TemplateNotFoundException"/>.
/// </para>
/// <example>
/// <code>
/// var provider = new SaasTemplateProvider();
/// var manifest = await provider.GetManifestAsync("saas");
/// var files = await provider.GetFilesAsync(manifest);
/// </code>
/// </example>
/// </remarks>
public sealed class SaasTemplateProvider : ITemplateProvider
{
    /// <summary>
    /// Gets the unique provider name: <c>"saas-official"</c>.
    /// </summary>
    public string Name => "saas-official";

    /// <summary>
    /// Retrieves the parsed <see cref="TemplateManifest"/> for the specified template.
    /// </summary>
    /// <param name="templateName">The name of the template to resolve.</param>
    /// <param name="version">An optional semantic version constraint. If <c>null</c>, the latest version is returned.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The parsed template manifest.</returns>
    /// <exception cref="TemplateNotFoundException">Thrown when <paramref name="templateName"/> is not <c>"saas"</c>.</exception>
    public Task<TemplateManifest> GetManifestAsync(
        string templateName,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        if (templateName != "saas")
            throw new TemplateNotFoundException(templateName, version);

        var manifest = SaasTemplateManifest.Create();
        return Task.FromResult(manifest);
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
        var files = SaasTemplateFiles.GetAllFiles();
        return Task.FromResult(files);
    }

    /// <summary>
    /// Checks whether a template with the given name exists in this provider.
    /// </summary>
    /// <param name="templateName">The name of the template to check.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><c>true</c> if <paramref name="templateName"/> is <c>"saas"</c>; otherwise <c>false</c>.</returns>
    public Task<bool> ExistsAsync(string templateName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(templateName == "saas");
    }
}
