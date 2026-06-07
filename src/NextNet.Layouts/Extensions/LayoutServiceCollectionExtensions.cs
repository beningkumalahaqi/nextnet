using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace NextNet.Layouts;

/// <summary>
/// Extension methods for registering NextNet Layouts services with the DI container.
/// </summary>
public static class LayoutServiceCollectionExtensions
{
    /// <summary>
    /// Registers NextNet Layouts services including <see cref="LayoutChainResolver"/>,
    /// <see cref="LayoutRenderer"/>, and <see cref="ErrorBoundaryRenderer"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The same service collection so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddNextNetLayouts(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // LayoutChainResolver requires IRouteComponentResolver from NextNet.Rendering
        services.TryAddScoped<LayoutChainResolver>();
        services.TryAddScoped<LayoutRenderer>();
        services.TryAddScoped<ErrorBoundaryRenderer>();

        return services;
    }
}
