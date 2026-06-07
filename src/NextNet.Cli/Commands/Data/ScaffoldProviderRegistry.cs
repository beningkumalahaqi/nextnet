using NextNet.Data.Abstractions.Abstractions;
using NextNet.Data.Abstractions.Internal;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Registry that maps data provider configuration keys to their corresponding
/// <see cref="IScaffoldProvider"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// Provider resolution follows this order:
/// <list type="number">
///   <item><description>Check <c>nextnet.config.json</c> for <c>data.provider</c> key.</description></item>
///   <item><description>If provider is set, return the matching <see cref="IScaffoldProvider"/>.</description></item>
///   <item><description>If provider is null, return <see cref="ModelOnlyScaffoldProvider"/> (models only).</description></item>
/// </list>
/// </para>
/// </remarks>
internal static class ScaffoldProviderRegistry
{
    private static readonly Dictionary<string, Func<IScaffoldProvider>> _providers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ef"] = CreateLazyEfProvider,
        ["dapper"] = CreateLazyDapperProvider,
        ["mongo"] = CreateLazyMongoProvider,
    };

    /// <summary>
    /// Gets the known provider keys that support scaffolding.
    /// </summary>
    public static IEnumerable<string> KnownProviders => _providers.Keys;

    /// <summary>
    /// Resolves an <see cref="IScaffoldProvider"/> for the given provider key.
    /// </summary>
    /// <param name="providerKey">The data provider key (e.g., "ef", "dapper", "mongo"). Case-insensitive.</param>
    /// <returns>The scaffold provider, or null if the key is unknown.</returns>
    public static IScaffoldProvider? GetProvider(string? providerKey)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
            return null;

        if (_providers.TryGetValue(providerKey, out var factory))
        {
            try
            {
                return factory();
            }
            catch
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a provider-agnostic scaffold provider that generates models only.
    /// Used when no data provider has been configured.
    /// </summary>
    public static IScaffoldProvider GetModelOnlyProvider() => new ModelOnlyScaffoldProvider();

    private static IScaffoldProvider CreateLazyEfProvider()
    {
        // Use reflection to avoid hard dependency when EF Core package is not installed
        var providerType = typeof(NextNet.Data.EntityFramework.EfCoreScaffoldProvider);
        return (IScaffoldProvider)Activator.CreateInstance(providerType)!;
    }

    private static IScaffoldProvider CreateLazyDapperProvider()
    {
        throw new NotSupportedException(
            "Dapper scaffold provider is not available. The NextNet.Data.Dapper package is not installed.");
    }

    private static IScaffoldProvider CreateLazyMongoProvider()
    {
        throw new NotSupportedException(
            "MongoDB scaffold provider is not available. The NextNet.Data.MongoDB package is not installed.");
    }
}
