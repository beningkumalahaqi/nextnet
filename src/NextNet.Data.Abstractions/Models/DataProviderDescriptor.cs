namespace NextNet.Data.Abstractions.Models;

/// <summary>
/// Describes a registered data provider, including its name and connection assignments.
/// Used for diagnostic and administrative purposes.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="DataProviderDescriptor"/> is used by the admin dashboard and diagnostic
/// tooling to display information about registered providers. It captures the provider's
/// runtime state including initialization status and last health check timestamp.
/// </para>
/// <example>
/// <code>
/// var descriptor = new DataProviderDescriptor(
///     "EntityFramework",
///     typeof(EntityFrameworkProvider),
///     new[] { "Default", "Analytics" },
///     isInitialized: true,
///     lastHealthCheck: DateTime.UtcNow);
/// </code>
/// </example>
/// </remarks>
/// <param name="Name">The provider name.</param>
/// <param name="ProviderType">The CLR type of the provider implementation.</param>
/// <param name="Connections">The connection names assigned to this provider.</param>
/// <param name="IsInitialized">Whether the provider has been initialized.</param>
/// <param name="LastHealthCheck">The timestamp of the last health check, if any.</param>
public sealed record DataProviderDescriptor(
    string Name,
    Type ProviderType,
    IReadOnlyList<string> Connections,
    bool IsInitialized = false,
    DateTime? LastHealthCheck = null
);
