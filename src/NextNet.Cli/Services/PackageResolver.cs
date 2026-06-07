namespace NextNet.Cli.Services;

/// <summary>
/// Maps data provider names to their corresponding NuGet package IDs and metadata.
/// Supports providers: ef (Entity Framework Core), dapper, mongo (MongoDB Driver).
/// </summary>
public static class PackageResolver
{
    /// <summary>
    /// Represents resolved package metadata for a data provider.
    /// </summary>
    /// <param name="ProviderName">The canonical provider name.</param>
    /// <param name="PackageId">The NuGet package ID.</param>
    /// <param name="Description">Human-readable description of the provider.</param>
    /// <param name="AdditionalPackages">Optional additional packages required by the provider.</param>
    public sealed record PackageInfo(
        string ProviderName,
        string PackageId,
        string Description,
        string[]? AdditionalPackages = null);

    private static readonly Dictionary<string, PackageInfo> KnownProviders = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ef"] = new PackageInfo(
            ProviderName: "ef",
            PackageId: "Microsoft.EntityFrameworkCore",
            Description: "Entity Framework Core — object-relational mapper for .NET",
            AdditionalPackages: new[] { "Microsoft.EntityFrameworkCore.Sqlite" }),

        ["dapper"] = new PackageInfo(
            ProviderName: "dapper",
            PackageId: "Dapper",
            Description: "Dapper — high-performance micro-ORM for .NET"),

        ["mongo"] = new PackageInfo(
            ProviderName: "mongo",
            PackageId: "MongoDB.Driver",
            Description: "MongoDB .NET Driver — official MongoDB client for .NET")
    };

    /// <summary>
    /// Gets the list of all supported provider names.
    /// </summary>
    public static string[] GetKnownProviders() => KnownProviders.Keys.ToArray();

    /// <summary>
    /// Resolves a provider name to its <see cref="PackageInfo"/>, or null if unknown.
    /// </summary>
    /// <param name="providerName">The provider name (case-insensitive).</param>
    /// <returns>The package info, or null if the provider is not recognized.</returns>
    public static PackageInfo? Resolve(string? providerName)
    {
        if (string.IsNullOrWhiteSpace(providerName))
            return null;

        return KnownProviders.TryGetValue(providerName, out var info) ? info : null;
    }

    /// <summary>
    /// Validates whether the given provider name is supported.
    /// </summary>
    public static bool IsValidProvider(string? providerName)
        => providerName is not null && KnownProviders.ContainsKey(providerName);
}
