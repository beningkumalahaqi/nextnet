namespace NextNet.Data.Internal;

/// <summary>
/// Internal descriptor that holds deferred registration state for a provider.
/// Stored in a list on <see cref="NextNetDataBuilder"/> and applied to the
/// DI container during <see cref="NextNetDataBuilder.Build"/>.
/// </summary>
/// <param name="Name">The provider name (e.g., "EntityFramework").</param>
/// <param name="ProviderType">The CLR type implementing <see cref="IDataProvider"/>.</param>
/// <param name="Options">The registration options for this provider.</param>
/// <param name="ConnectionName">The connection name for named provider registrations, or <c>null</c>.</param>
/// <param name="ConnectionString">The explicit connection string, or <c>null</c>.</param>
internal sealed record ProviderRegistrationDescriptor(
    string Name,
    Type ProviderType,
    ProviderRegistrationOptions Options,
    string? ConnectionName,
    string? ConnectionString)
{
    /// <summary>
    /// Gets whether this is a named provider registration (has an explicit connection name).
    /// </summary>
    public bool IsNamedProvider => ConnectionName is not null;
}
