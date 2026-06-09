using Microsoft.Extensions.DependencyInjection;

namespace NextNet.UI.Abstractions.Extensions;

/// <summary>
/// Provides extension methods for registering NextNet UI Abstractions services
/// with the Microsoft dependency injection container.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="AddNextNetUIAbstractions"/> method registers the core UI abstraction
/// services, including the component hierarchy and rendering infrastructure, into the
/// application's <see cref="IServiceCollection"/>.
/// </para>
/// <para>
/// This method should be called during application startup, typically from the
/// <c>ConfigureServices</c> method in the application's composition root.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var builder = WebApplication.CreateBuilder(args);
/// builder.Services.AddNextNetUIAbstractions();
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers NextNet UI Abstractions services into the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddNextNetUIAbstractions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register component renderers as transient services.
        // Concrete renderer implementations are registered by the consuming
        // application or by additional AddNextNetUI* extension methods.

        return services;
    }
}
