namespace NextNet.Data.MultiDb;

/// <summary>
/// Configuration options for the Multi-Database subsystem.
/// Controls connection resolution behavior, caching, and validation settings.
/// </summary>
/// <remarks>
/// <para>
/// These options can be configured via <c>nextnet.config.json</c> or programmatically
/// using the builder extension methods. They control how <see cref="DatabaseSelector"/>
/// resolves named connections and manages connection contexts.
/// </para>
/// <example>
/// <code>
/// builder.Services.AddNextNetData()
///     .WithDatabaseSelector(opts =>
///     {
///         opts.ValidateOnStartup = true;
///         opts.CacheContexts = true;
///         opts.FallbackToDefault = false;
///     });
/// </code>
/// </example>
/// </remarks>
public sealed record MultiDbOptions
{
    /// <summary>
    /// Gets or sets whether to validate all connection names at startup (eager validation).
    /// When <c>true</c>, missing or misconfigured connections throw during application startup.
    /// When <c>false</c>, errors are deferred until <see cref="IDatabaseSelector.For"/> is called.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool ValidateOnStartup { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to cache resolved database contexts for the lifetime of the
    /// selector. When <c>true</c>, each call to <c>For("name")</c> returns a cached context.
    /// When <c>false</c>, a new context is created each time.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool CacheContexts { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to allow falling back to the default connection when a
    /// requested named connection is not found.
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool FallbackToDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets the default connection name used when <see cref="IDatabaseSelector.Default"/>
    /// is accessed. If not set, uses <see cref="DataConfig.DefaultConnection"/>.
    /// </summary>
    public string? DefaultConnectionName { get; set; }
}
