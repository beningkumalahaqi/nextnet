namespace NextNet.Data.MultiDb.Internal;

/// <summary>
/// Describes how a named connection is registered.
/// Stored in <see cref="ConnectionNameRegistry"/> for runtime resolution.
/// </summary>
/// <param name="ConnectionName">The logical connection name (e.g., "Analytics", "Primary").</param>
/// <param name="ProviderName">The name of the data provider (e.g., "EntityFramework", "Dapper").</param>
/// <param name="ConnectionString">The resolved connection string.</param>
/// <param name="ProviderType">The CLR type implementing <see cref="IDataProvider"/>.</param>
/// <param name="IsInitialized">Whether the connection has been initialized.</param>
internal sealed record ConnectionRegistration(
    string ConnectionName,
    string ProviderName,
    string ConnectionString,
    Type ProviderType,
    bool IsInitialized = false);
