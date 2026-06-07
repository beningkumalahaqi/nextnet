namespace NextNet.Data;

/// <summary>
/// Metadata describing a data provider. Used by the CLI for listing, diagnostics,
/// and scaffolding decisions without instantiating the provider.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ProviderMetadata"/> can be obtained from a provider type via the
/// <see cref="Extensions.ProviderMetadataExtensions.GetMetadata"/> extension method,
/// which reads the <see cref="ProviderMetadataAttribute"/> applied to the provider class.
/// </para>
/// <para>
/// This metadata is discoverable at the assembly level, enabling the CLI to list
/// available providers without loading their dependencies.
/// </para>
/// <example>
/// <code>
/// var metadata = typeof(EntityFrameworkProvider).GetMetadata();
/// Console.WriteLine($"Provider: {metadata.DisplayName} (v{metadata.Version})");
/// Console.WriteLine($"Databases: {string.Join(", ", metadata.SupportedDatabases)}");
/// </code>
/// </example>
/// </remarks>
public sealed record ProviderMetadata
{
    /// <summary>
    /// Gets the unique provider identifier (e.g., "EntityFramework").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets the human-readable display name (e.g., "Entity Framework Core 10").
    /// </summary>
    public required string DisplayName { get; init; }

    /// <summary>
    /// Gets the provider version.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Gets a short description of the provider.
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// Gets the NuGet package name for installing this provider
    /// (e.g., "NextNet.Data.EntityFramework").
    /// </summary>
    public required string PackageName { get; init; }

    /// <summary>
    /// Gets the CLI command to scaffold this provider
    /// (e.g., "nextnet add data ef").
    /// </summary>
    public required string CliCommand { get; init; }

    /// <summary>
    /// Gets the list of supported database platforms
    /// (e.g., "SQL Server", "PostgreSQL", "MongoDB").
    /// </summary>
    public IReadOnlyList<string> SupportedDatabases { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Gets whether this provider supports database migrations.
    /// </summary>
    public bool SupportsMigrations { get; init; }

    /// <summary>
    /// Gets whether this provider supports the repository pattern.
    /// </summary>
    public bool SupportsRepositories { get; init; }
}
