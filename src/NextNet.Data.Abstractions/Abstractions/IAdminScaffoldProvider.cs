using NextNet.Data.Abstractions.Models;

namespace NextNet.Data.Abstractions.Abstractions;

/// <summary>
/// Extends <see cref="IScaffoldProvider"/> with admin page generation capabilities.
/// Implementations are provider-aware and generate admin pages that use
/// the provider's repository pattern for data access.
/// </summary>
public interface IAdminScaffoldProvider : IScaffoldProvider
{
    /// <summary>
    /// Generates a full set of admin CRUD pages for the specified entity.
    /// </summary>
    /// <param name="entityName">The PascalCase entity name.</param>
    /// <param name="options">Scaffold options for output paths and behavior.</param>
    /// <param name="adminOptions">Admin-specific options (route prefix, layout name, etc.).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of artifacts describing each generated file.</returns>
    Task<ScaffoldArtifact[]> GenerateAdminPagesAsync(
        string entityName,
        ScaffoldOptions options,
        AdminScaffoldOptions adminOptions,
        CancellationToken cancellationToken = default);
}
