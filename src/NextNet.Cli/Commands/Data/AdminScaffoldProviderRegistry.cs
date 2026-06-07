using NextNet.Data.Abstractions.Abstractions;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Maps data provider keys to <see cref="IAdminScaffoldProvider"/> implementations.
/// Used by the <c>nextnet generate admin</c> command to resolve the correct
/// provider-specific admin page generator.
/// </summary>
/// <remarks>
/// <para>
/// The registry performs lazy instantiation to avoid hard dependencies on
/// provider assemblies that may not be installed in the target project.
/// </para>
/// </remarks>
internal static class AdminScaffoldProviderRegistry
{
    private static readonly Dictionary<string, Func<IAdminScaffoldProvider>> _providers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ef"] = CreateEfAdminScaffoldProvider,
        ["entityframework"] = CreateEfAdminScaffoldProvider,
        ["dapper"] = CreateDapperAdminScaffoldProvider,
    };

    /// <summary>
    /// Gets an <see cref="IAdminScaffoldProvider"/> for the given provider key.
    /// </summary>
    /// <param name="providerKey">The data provider key (e.g., "ef", "dapper"). Case-insensitive.</param>
    /// <returns>The admin scaffold provider, or null if the key is unknown or unavailable.</returns>
    public static IAdminScaffoldProvider? GetProvider(string? providerKey)
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
    /// Gets all registered provider keys that support admin scaffolding.
    /// </summary>
    public static IEnumerable<string> GetSupportedProviderKeys() => _providers.Keys;

    private static IAdminScaffoldProvider CreateEfAdminScaffoldProvider()
    {
        var type = typeof(NextNet.Data.EntityFramework.Admin.EfCoreAdminScaffoldProvider);
        return (IAdminScaffoldProvider)Activator.CreateInstance(type)!;
    }

    private static IAdminScaffoldProvider CreateDapperAdminScaffoldProvider()
    {
        var type = typeof(NextNet.Data.Dapper.Admin.DapperAdminScaffoldProvider);
        return (IAdminScaffoldProvider)Activator.CreateInstance(type)!;
    }
}
