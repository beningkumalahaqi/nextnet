using Microsoft.Extensions.DependencyInjection;
using NextNet.DesignSystem.Defaults;
using NextNet.DesignSystem.Tokens;
using NextNet.UI.Abstractions.Theme;
using NextNet.UI.Theming.Presets;

namespace NextNet.UI.Theming.Extensions;

/// <summary>
/// Provides extension methods for registering the NextNet theming system into the
/// dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AddNextNetTheming"/> registers the following services:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="ISystemPreferenceResolver"/> as a singleton (defaults to <see cref="DefaultSystemPreferenceResolver"/>)</description></item>
///   <item><description><see cref="ThemeManager"/> as a singleton implementation of <see cref="IThemeProvider"/></description></item>
///   <item><description><see cref="ThemeOptions"/> as a singleton configuration object</description></item>
/// </list>
/// <para>
/// The built-in light and dark theme presets are registered by default.
/// When <see cref="ThemeOptions.DarkMode"/> is set to <see cref="DarkMode.System"/>,
/// the active theme is resolved from the OS preference via <see cref="ISystemPreferenceResolver"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Minimal registration with defaults (light + dark themes)
/// services.AddNextNetTheming();
///
/// // Custom configuration with System mode
/// services.AddNextNetTheming(options =>
/// {
///     options.DarkMode = DarkMode.System;
/// });
///
/// // Custom default theme
/// services.AddNextNetTheming(options =>
/// {
///     options.DefaultThemeName = "dark";
/// });
/// </code>
/// </example>
public static class ThemeServiceExtensions
{
    /// <summary>
    /// Registers the NextNet theming services, including <see cref="ThemeManager"/> as
    /// a singleton <see cref="IThemeProvider"/>, and pre-registers the built-in light
    /// and dark theme presets.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="ThemeOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetTheming(
        this IServiceCollection services,
        Action<ThemeOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new ThemeOptions();
        configureOptions?.Invoke(options);

        services.AddSingleton(options);

        // Register the system preference resolver (can be overridden by consumers)
        services.AddSingleton<ISystemPreferenceResolver, DefaultSystemPreferenceResolver>();

        services.AddSingleton<IThemeProvider>(sp =>
        {
            var resolver = sp.GetRequiredService<ISystemPreferenceResolver>();
            var manager = new ThemeManager(resolver);

            // Load theme overrides from nextnet.theme.json if configured
            DesignTokenSet baseTokens;
            if (!string.IsNullOrWhiteSpace(options.ThemeJsonBasePath))
            {
                var loader = new ThemeJsonLoader(options.ThemeJsonBasePath);
                baseTokens = loader.Load();
            }
            else
            {
                baseTokens = DefaultTokens.Create();
            }

            // Register built-in theme presets (using overridden tokens if available)
            manager.RegisterTheme(LightTheme.Create(baseTokens));
            manager.RegisterTheme(DarkTheme.Create(baseTokens));

            // Set the default theme respecting DarkMode
            if (options.DarkMode == DarkMode.System)
            {
                manager.SetDarkMode(DarkMode.System);
            }
            else if (options.DarkMode == DarkMode.Dark)
            {
                manager.SetDarkMode(DarkMode.Dark);
            }
            else
            {
                manager.SetActiveTheme(options.DefaultThemeName);
            }

            return manager;
        });

        return services;
    }
}
