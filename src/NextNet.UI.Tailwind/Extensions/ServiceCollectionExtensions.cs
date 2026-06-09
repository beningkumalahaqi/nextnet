using Microsoft.Extensions.DependencyInjection;
using NextNet.UI.Abstractions.Components;
using NextNet.UI.Abstractions.Rendering;
using NextNet.UI.Tailwind.Config;
using NextNet.UI.Tailwind.Css;
using NextNet.UI.Tailwind.Mapping;

namespace NextNet.UI.Tailwind.Extensions;

/// <summary>
/// Provides extension methods for registering the NextNet Tailwind integration
/// services into the dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="AddNextNetTailwind"/> registers the following services:
/// </para>
/// <list type="bullet">
///   <item><description><see cref="TailwindOptions"/> — singleton configuration</description></item>
///   <item><description><see cref="ClassMapperRegistry"/> — singleton</description></item>
///   <item><description><see cref="TailwindStyleBuilder"/> — singleton</description></item>
///   <item><description><see cref="TailwindConfigGenerator"/> — static utility (no registration needed)</description></item>
///   <item><description>Built-in component class mappers for Button, Card, Input, Badge, and Alert</description></item>
/// </list>
/// <para>
/// Mappers are registered in the registry for the following component interfaces:
/// <see cref="IButton"/>, <see cref="ICard"/>, <see cref="IInput"/>, <see cref="IBadge"/>,
/// and <see cref="IAlert"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Minimal registration with defaults
/// services.AddNextNetTailwind();
///
/// // Custom configuration
/// services.AddNextNetTailwind(options =>
/// {
///     options.ContentPaths = new[] { "./Pages/**/*.cshtml" };
///     options.SafelistPatterns = new[] { "btn-*", "badge-*" };
/// });
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the NextNet Tailwind integration services into the specified
    /// <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configureOptions">An optional delegate to configure <see cref="TailwindOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetTailwind(
        this IServiceCollection services,
        Action<TailwindOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Configure options
        var options = new TailwindOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register style builder
        services.AddSingleton<TailwindStyleBuilder>();

        // Register and populate the class mapper registry
        var registry = new ClassMapperRegistry();

        registry.Register<IButton>(new ButtonClassMapper());
        registry.Register<ICard>(new CardClassMapper());
        registry.Register<IInput>(new InputClassMapper());
        registry.Register<IBadge>(new BadgeClassMapper());
        registry.Register<IAlert>(new AlertClassMapper());

        services.AddSingleton(registry);

        return services;
    }
}
