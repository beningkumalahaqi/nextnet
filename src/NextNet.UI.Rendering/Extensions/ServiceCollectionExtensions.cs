using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Rendering.Composition;
using NextNet.UI.Rendering.Head;

namespace NextNet.UI.Rendering.Extensions;

/// <summary>
/// Extension methods for registering NextNet UI Rendering services with the
/// dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ServiceCollectionExtensions"/> provides the <c>AddNextNetUiRendering</c>
/// method that registers all services required for UI rendering, including:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ComponentTreeRenderer"/> for rendering component trees</description></item>
///   <item><description><see cref="ThemeHeadInjector"/> for injecting theme CSS</description></item>
///   <item><description><see cref="IHeadContentProvider"/> implementations</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs:
/// builder.Services.AddNextNetUiRendering(options =>
/// {
///     options.DefaultTheme = "light";
/// });
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet UI Rendering services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">An optional delegate to configure UI rendering options.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddNextNetUiRendering(
        this IServiceCollection services,
        Action<UiRenderingOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Configure options
        var options = new UiRenderingOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Register core rendering services
        services.AddScoped<ComponentTreeRenderer>();
        services.AddSingleton<ThemeHeadInjector>();

        // Register head content provider if specified
        if (options.HeadContentProviderType != null)
        {
            services.AddScoped(typeof(IHeadContentProvider), options.HeadContentProviderType);
        }

        // Register pages for DI
        services.AddScoped<Pages.UiPage>();
        services.AddScoped(typeof(Pages.UiPage<>));

        // Register layout
        services.AddScoped<Layouts.UiLayout>();

        return services;
    }
}

/// <summary>
/// Configuration options for the NextNet UI Rendering system.
/// </summary>
/// <remarks>
/// These options control the default theme, head content provider type, and
/// other UI rendering behaviors.
/// </remarks>
public sealed class UiRenderingOptions
{
    /// <summary>
    /// Gets or sets the default theme name used when no theme is explicitly specified.
    /// Defaults to <c>"light"</c>.
    /// </summary>
    public string DefaultTheme { get; set; } = "light";

    /// <summary>
    /// Gets or sets the <see cref="Type"/> of the <see cref="IHeadContentProvider"/>
    /// implementation to register. If null, no custom head content provider is registered.
    /// </summary>
    public Type? HeadContentProviderType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the footer should be shown by default
    /// in layouts that support it.
    /// </summary>
    public bool ShowFooter { get; set; } = true;

    /// <summary>
    /// Gets or sets the default footer content HTML. If null, a default copyright footer is used.
    /// </summary>
    public string? DefaultFooterContent { get; set; }
}
