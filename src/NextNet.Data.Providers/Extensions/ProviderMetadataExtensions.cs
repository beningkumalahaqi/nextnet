using System.Reflection;

namespace NextNet.Data.Extensions;

/// <summary>
/// Extension methods for working with <see cref="ProviderMetadata"/> and
/// <see cref="ProviderMetadataAttribute"/>.
/// </summary>
/// <remarks>
/// <para>
/// These extensions enable reflection-based discovery of provider metadata without
/// instantiating the provider. This is used by the NextNet CLI to list available
/// providers and their capabilities.
/// </para>
/// <example>
/// <code>
/// var metadata = typeof(EntityFrameworkProvider).GetMetadata();
/// Console.WriteLine($"Provider: {metadata.DisplayName} (v{metadata.Version})");
/// </code>
/// </example>
/// </remarks>
public static class ProviderMetadataExtensions
{
    /// <summary>
    /// Gets the <see cref="ProviderMetadata"/> for the specified provider type by
    /// reading its <see cref="ProviderMetadataAttribute"/>.
    /// </summary>
    /// <param name="providerType">The CLR type of the provider.</param>
    /// <returns>
    /// A <see cref="ProviderMetadata"/> instance populated from the type's
    /// <see cref="ProviderMetadataAttribute"/>, or <c>null</c> if no attribute is applied.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerType"/> is <c>null</c>.</exception>
    /// <example>
    /// <code>
    /// var metadata = typeof(EntityFrameworkProvider).GetMetadata();
    /// if (metadata is not null)
    /// {
    ///     Console.WriteLine($"Found provider: {metadata.DisplayName}");
    /// }
    /// </code>
    /// </example>
    public static ProviderMetadata? GetMetadata(this Type providerType)
    {
        ArgumentNullException.ThrowIfNull(providerType);

        var attribute = providerType.GetCustomAttribute<ProviderMetadataAttribute>();

        if (attribute is null)
            return null;

        // Try to get the version from the provider type's assembly
        var assemblyVersion = providerType.Assembly.GetName().Version ?? new Version(1, 0, 0);

        return new ProviderMetadata
        {
            Id = attribute.Id,
            DisplayName = attribute.DisplayName,
            Version = assemblyVersion,
            Description = attribute.Description,
            PackageName = attribute.PackageName,
            CliCommand = attribute.CliCommand,
            SupportedDatabases = attribute.SupportedDatabases ?? Array.Empty<string>(),
            SupportsMigrations = attribute.SupportsMigrations,
            SupportsRepositories = attribute.SupportsRepositories
        };
    }

    /// <summary>
    /// Gets the <see cref="ProviderMetadataAttribute"/> applied to the specified
    /// provider type, or <c>null</c> if none is applied.
    /// </summary>
    /// <param name="providerType">The CLR type of the provider.</param>
    /// <returns>
    /// The <see cref="ProviderMetadataAttribute"/> instance, or <c>null</c> if not found.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="providerType"/> is <c>null</c>.</exception>
    public static ProviderMetadataAttribute? GetMetadataAttribute(this Type providerType)
    {
        ArgumentNullException.ThrowIfNull(providerType);

        return providerType.GetCustomAttribute<ProviderMetadataAttribute>();
    }
}
