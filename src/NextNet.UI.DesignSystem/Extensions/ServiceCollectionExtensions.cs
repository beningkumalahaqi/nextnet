using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.DesignSystem.Components;
using NextNet.UI.DesignSystem.Rendering;

namespace NextNet.UI.DesignSystem.Extensions;

/// <summary>
/// Provides extension methods for registering the NextNet Design System services
/// with the Microsoft dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="AddNextNetDesignSystem"/> method registers the design system's
/// component implementations, rendering infrastructure, and configuration options
/// into the application's <see cref="IServiceCollection"/>.
/// </para>
/// <para>
/// This method should be called during application startup after the core
/// UI Abstractions and Theming services have been registered.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// builder.Services.AddNextNetDesignSystem(options =>
/// {
///     options.DefaultThemeName = "dark";
///     options.AutoRegisterComponents = true;
/// });
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers NextNet Design System services into the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="DesignSystemOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddNextNetDesignSystem(
        this IServiceCollection services,
        Action<DesignSystemOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = new DesignSystemOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register the component renderer registry as a singleton.
        services.AddSingleton<ComponentRendererRegistry>();

        // Register the default component renderer for all standard component types.
        services.AddSingleton<IComponentRenderer<IButton>, DefaultComponentRenderer<IButton>>();
        services.AddSingleton<IComponentRenderer<ICard>, DefaultComponentRenderer<ICard>>();
        services.AddSingleton<IComponentRenderer<IInput>, DefaultComponentRenderer<IInput>>();
        services.AddSingleton<IComponentRenderer<IBadge>, DefaultComponentRenderer<IBadge>>();
        services.AddSingleton<IComponentRenderer<IAvatar>, DefaultComponentRenderer<IAvatar>>();
        services.AddSingleton<IComponentRenderer<IAlert>, DefaultComponentRenderer<IAlert>>();
        services.AddSingleton<IComponentRenderer<IModal>, DefaultComponentRenderer<IModal>>();
        services.AddSingleton<IComponentRenderer<IDropdown>, DefaultComponentRenderer<IDropdown>>();
        services.AddSingleton<IComponentRenderer<ITable>, DefaultComponentRenderer<ITable>>();
        services.AddSingleton<IComponentRenderer<ITabs>, DefaultComponentRenderer<ITabs>>();
        services.AddSingleton<IComponentRenderer<IToggle>, DefaultComponentRenderer<IToggle>>();

        return services;
    }
}
