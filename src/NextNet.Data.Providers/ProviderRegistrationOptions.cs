namespace NextNet.Data;

/// <summary>
/// Options for registering a single provider instance.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ProviderRegistrationOptions"/> is used by <see cref="NextNetDataBuilder.AddProvider{TProvider}"/>
/// and <see cref="NextNetDataBuilder.AddNamedProvider{TProvider}"/> to configure how a
/// specific provider is registered in the dependency injection container.
/// </para>
/// <para>
/// When both <see cref="ConnectionString"/> and <see cref="ConnectionStringName"/> are set,
/// the explicit <see cref="ConnectionString"/> takes precedence.
/// </para>
/// <example>
/// <code>
/// builder.AddProvider&lt;EntityFrameworkProvider&gt;("EntityFramework", opts =>
/// {
///     opts.ConnectionStringName = "DefaultConnection";
///     opts.RegisterHealthChecks = true;
///     opts.Lifetime = ServiceLifetime.Singleton;
/// });
/// </code>
/// </example>
/// </remarks>
public sealed record ProviderRegistrationOptions
{
    /// <summary>
    /// Gets or sets the connection string name from configuration (e.g., "DefaultConnection").
    /// This is used to look up the connection string from application configuration at runtime.
    /// If <see cref="ConnectionString"/> is also set, this value is ignored.
    /// </summary>
    public string? ConnectionStringName { get; set; }

    /// <summary>
    /// Gets or sets an explicit connection string.
    /// Takes precedence over <see cref="ConnectionStringName"/> when both are set.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets whether to register health checks for this provider.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool RegisterHealthChecks { get; set; } = true;

    /// <summary>
    /// Gets or sets the service lifetime for the provider instance.
    /// Defaults to <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Singleton;
}
