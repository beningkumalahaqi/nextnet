namespace NextNet.Data;

/// <summary>
/// Attribute applied to provider classes to expose metadata at the assembly level.
/// Used by CLI discovery and diagnostic tools without instantiating the provider.
/// </summary>
/// <remarks>
/// <para>
/// Apply this attribute to each concrete <see cref="IDataProvider"/> implementation
/// to enable tooling like the NextNet CLI to list available providers, their
/// supported databases, and scaffolding capabilities without loading provider assemblies.
/// </para>
/// <para>
/// The <c>GetMetadata</c> extension method in <see cref="NextNet.Data.Extensions.ProviderMetadataExtensions"/>
/// reads this attribute from a provider type and returns a <see cref="ProviderMetadata"/> record.
/// </para>
/// <example>
/// <code>
/// [ProviderMetadata(
///     "EntityFramework",
///     "Entity Framework Core 10",
///     "Full-featured ORM provider based on Entity Framework Core")]
/// public class EntityFrameworkProvider : IDataProvider
/// {
///     // ...
/// }
/// </code>
/// </example>
/// </remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ProviderMetadataAttribute : Attribute
{
    /// <summary>
    /// Gets the unique provider identifier (e.g., "EntityFramework").
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the human-readable display name (e.g., "Entity Framework Core 10").
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets a short description of the provider.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets or sets the NuGet package name for installing this provider
    /// (e.g., "NextNet.Data.EntityFramework").
    /// </summary>
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the CLI command to scaffold this provider
    /// (e.g., "nextnet add data ef").
    /// </summary>
    public string CliCommand { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of supported database platforms.
    /// </summary>
    public string[] SupportedDatabases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets whether this provider supports database migrations.
    /// </summary>
    public bool SupportsMigrations { get; set; }

    /// <summary>
    /// Gets or sets whether this provider supports the repository pattern.
    /// </summary>
    public bool SupportsRepositories { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderMetadataAttribute"/> class.
    /// </summary>
    /// <param name="id">The unique provider identifier (e.g., "EntityFramework").</param>
    /// <param name="displayName">The human-readable display name for the provider.</param>
    /// <param name="description">A short description of the provider's capabilities.</param>
    public ProviderMetadataAttribute(string id, string displayName, string description)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
    }
}
