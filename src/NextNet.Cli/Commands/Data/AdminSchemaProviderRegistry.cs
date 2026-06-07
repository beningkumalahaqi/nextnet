using NextNet.Data.Abstractions.Abstractions;

namespace NextNet.Cli.Commands.Data;

/// <summary>
/// Maps data provider keys to <see cref="IAdminSchemaProvider"/> implementations.
/// Registered in the CLI for database exploration commands.
/// </summary>
/// <remarks>
/// <para>
/// The registry supports lazy resolution — providers are only instantiated
/// when requested. This avoids hard dependencies on provider-specific packages
/// that may not be installed in the target project.
/// </para>
/// </remarks>
internal static class AdminSchemaProviderRegistry
{
    private static readonly Dictionary<string, Func<IAdminSchemaProvider>> _providers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ef"] = CreateEfSchemaProvider,
        ["entityframework"] = CreateEfSchemaProvider,
        ["dapper"] = CreateDapperSchemaProvider,
    };

    /// <summary>
    /// Gets an <see cref="IAdminSchemaProvider"/> for the given provider key.
    /// </summary>
    /// <param name="providerKey">The data provider key (e.g., "ef", "dapper"). Case-insensitive.</param>
    /// <returns>The schema provider, or null if the key is unknown or the provider assembly is not available.</returns>
    public static IAdminSchemaProvider? GetProvider(string? providerKey)
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
    /// Gets all registered provider keys that support schema exploration.
    /// </summary>
    public static IEnumerable<string> GetSupportedProviderKeys() => _providers.Keys;

    private static IAdminSchemaProvider CreateEfSchemaProvider()
    {
        return new NextNet.Data.EntityFramework.Admin.EfCoreAdminSchemaProvider();
    }

    private static IAdminSchemaProvider CreateDapperSchemaProvider()
    {
        return new NextNet.Data.Dapper.Admin.DapperAdminSchemaProvider(
            () => new Microsoft.Data.SqlClient.SqlConnection());
    }
}
