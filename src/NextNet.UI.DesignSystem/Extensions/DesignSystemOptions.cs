namespace NextNet.UI.DesignSystem.Extensions;

/// <summary>
/// Configuration options for the NextNet Design System.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DesignSystemOptions"/> allows consumers to customize the design system's
/// behavior, including the default theme name, available themes, and whether
/// component renderers are automatically registered in DI.
/// </para>
/// <para>
/// These options are typically set during service registration via
/// <see cref="ServiceCollectionExtensions.AddNextNetDesignSystem"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// services.AddNextNetDesignSystem(options =>
/// {
///     options.DefaultThemeName = "dark";
///     options.AutoRegisterComponents = true;
/// });
/// </code>
/// </example>
public sealed record DesignSystemOptions
{
    /// <summary>
    /// Gets or sets the name of the default theme used when no explicit theme is requested.
    /// Defaults to <c>"light"</c>.
    /// </summary>
    public string DefaultThemeName { get; set; } = "light";

    /// <summary>
    /// Gets or sets the list of available theme names for the application.
    /// Defaults to <c>["light", "dark"]</c>.
    /// </summary>
    public IReadOnlyList<string> AvailableThemes { get; set; } = new[] { "light", "dark" };

    /// <summary>
    /// Gets or sets a value indicating whether standard component renderers
    /// are automatically registered in the dependency injection container.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool AutoRegisterComponents { get; set; } = true;
}
