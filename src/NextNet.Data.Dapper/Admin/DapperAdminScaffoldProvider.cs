using System.Data;
using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Internal;
using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Dapper.Admin;

/// <summary>
/// Dapper implementation of <see cref="IAdminScaffoldProvider"/> for generating
/// admin CRUD pages. Delegates model/repository generation to the standard
/// Dapper scaffold provider and generates admin pages using the same
/// template approach as the EF Core provider.
/// </summary>
public sealed class DapperAdminScaffoldProvider : IAdminScaffoldProvider
{
    /// <inheritdoc />
    public string ProviderName => "Dapper";

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateModelAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        // Delegate to standard model generation
        throw new NotSupportedException(
            "Dapper admin scaffold model generation is not yet implemented. " +
            "Use 'nextnet generate model' to create models separately.");
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact> GenerateRepositoryAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Dapper admin scaffold repository generation is not yet implemented. " +
            "Use 'nextnet generate repository' to create repositories separately.");
    }

    /// <inheritdoc />
    public Task<ScaffoldArtifact[]> GenerateCrudAsync(
        string entityName,
        ScaffoldOptions options,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException(
            "Dapper CRUD generation is not yet implemented. " +
            "Use 'nextnet generate crud' with EF Core provider instead.");
    }

    /// <inheritdoc />
    public async Task<ScaffoldArtifact[]> GenerateAdminPagesAsync(
        string entityName,
        ScaffoldOptions options,
        AdminScaffoldOptions adminOptions,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityName))
            throw new ArgumentNullException(nameof(entityName));

        // Use shared admin page generation logic
        var generator = new AdminPageGenerator(entityName, options, adminOptions, "Dapper");
        return await generator.GenerateAllAsync(cancellationToken);
    }
}
