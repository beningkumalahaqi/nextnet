namespace NextNet.Data.Providers;

/// <summary>
/// Defines standardized error codes for the NextNet Data Providers layer.
/// Each code corresponds to a specific exception type or error condition
/// and is embedded in exception messages as a prefix (e.g., "[DS-610]").
/// </summary>
/// <remarks>
/// <para>
/// These error codes are used throughout the NextNet Data framework to enable
/// programmatic error identification, automated handling, and diagnostic tooling.
/// Exception messages are prefixed with the error code in square brackets.
/// </para>
/// <para>
/// Old <c>SKDATA_PROVIDER_*</c> constants are retained as <c>[Obsolete]</c> aliases
/// for backward compatibility.
/// </para>
/// </remarks>
public static class DataProviderErrorCodes
{
    // ──────────────────────────────────────────────────────────────
    //  Current error codes (DS-610 … DS-619 range)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// The requested provider has not been registered in the provider registry.
    /// </summary>
    public const string ProviderNotRegistered = "DS-610";

    /// <summary>
    /// A provider with the same name has already been registered.
    /// </summary>
    public const string ProviderAlreadyRegistered = "DS-611";

    /// <summary>
    /// The provider configuration is missing or contains invalid values.
    /// </summary>
    public const string InvalidProviderConfiguration = "DS-612";

    /// <summary>
    /// The provider failed to initialize during application startup.
    /// </summary>
    public const string ProviderInitializationFailed = "DS-613";

    /// <summary>
    /// The requested operation is not supported by the provider.
    /// </summary>
    public const string ProviderNotSupported = "DS-614";

    // ──────────────────────────────────────────────────────────────
    //  Obsolete aliases (migrate from SKDATA_PROVIDER_* → DS-*)
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Obsolete. Use <see cref="ProviderAlreadyRegistered"/> instead.
    /// </summary>
    [Obsolete("Use " + nameof(ProviderAlreadyRegistered) + " instead. (DS-611)")]
    public const string SKDATA_PROVIDER_001 = "DS-611";

    /// <summary>
    /// Obsolete. Use <see cref="ProviderInitializationFailed"/> instead.
    /// </summary>
    [Obsolete("Use " + nameof(ProviderInitializationFailed) + " instead. (DS-613)")]
    public const string SKDATA_PROVIDER_002 = "DS-613";

    /// <summary>
    /// Obsolete. Use <see cref="ProviderNotRegistered"/> instead.
    /// </summary>
    [Obsolete("Use " + nameof(ProviderNotRegistered) + " instead. (DS-610)")]
    public const string SKDATA_PROVIDER_003 = "DS-610";

    /// <summary>
    /// Obsolete. Use <see cref="ProviderNotSupported"/> instead.
    /// </summary>
    [Obsolete("Use " + nameof(ProviderNotSupported) + " instead. (DS-614)")]
    public const string SKDATA_PROVIDER_004 = "DS-614";

    /// <summary>
    /// Obsolete. Use <see cref="InvalidProviderConfiguration"/> instead.
    /// </summary>
    [Obsolete("Use " + nameof(InvalidProviderConfiguration) + " instead. (DS-612)")]
    public const string SKDATA_PROVIDER_005 = "DS-612";
}
