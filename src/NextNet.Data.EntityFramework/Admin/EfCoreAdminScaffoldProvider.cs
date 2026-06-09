using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Internal;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.EntityFramework.Admin;

/// <summary>
/// EF Core implementation of <see cref="IAdminScaffoldProvider"/> for generating
/// admin CRUD pages (List, Detail, Create, Edit, Delete) plus an AdminLayout.
/// </summary>
/// <remarks>
/// <para>
/// Admin pages are generated as standard NextNet page components using <c>IPage</c>
/// and <c>ILayout</c>. They use the EF Core repository pattern for data access.
/// </para>
/// <para>
/// The generated pages are placed under <c>app/admin/{{EntityName}}/</c> and follow
/// NextNet's file-based routing conventions.
/// </para>
/// </remarks>
public sealed class EfCoreAdminScaffoldProvider : IAdminScaffoldProvider
{
    /// <inheritdoc />
    public string ProviderName => "EntityFramework";

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        var efProvider = new EfCoreScaffoldProvider();
        return efProvider.GenerateModelAsync(entityName, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        var efProvider = new EfCoreScaffoldProvider();
        return efProvider.GenerateRepositoryAsync(entityName, options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        var efProvider = new EfCoreScaffoldProvider();
        return efProvider.GenerateCrudAsync(entityName, options, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ScaffoldArtifact[]> GenerateAdminPagesAsync(
        string entityName,
        ScaffoldOptions options,
        AdminScaffoldOptions adminOptions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName), $"[{EntityFrameworkErrorCodes.ConfigurationInvalid}] Entity name must not be null or empty.");

        var generator = new AdminPageGenerator(entityName, options, adminOptions, "EntityFramework");
        return await generator.GenerateAllAsync(cancellationToken);
    }
}
