using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Configuration;
using NextNet.IO;
using NextNet.Logging;
using NextNet.Rendering.Streaming;

namespace NextNet.Rendering.Extensions;

/// <summary>
/// Extension methods for registering NextNet Rendering services with the DI container.
/// </summary>
public static class NextNetServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet SSR and rendering services to the service collection.
    /// Registers <see cref="SsrRenderer"/>, <see cref="SsrOptions"/>,
    /// <see cref="StreamingHtmlRenderer"/>, and related services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureSsr">Optional delegate to configure SSR options.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is <c>null</c>.</exception>
    public static IServiceCollection AddNextNetRendering(
        this IServiceCollection services,
        Action<SsrOptions>? configureSsr = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Configure SsrOptions
        var ssrOptions = new SsrOptions();
        configureSsr?.Invoke(ssrOptions);
        services.AddSingleton(ssrOptions);

        // Register default file system
        services.AddSingleton<ISharpFileSystem>(_ => new DefaultSharpFileSystem());

        // Register SSR renderer and supporting services
        services.AddScoped<SsrRenderer>();
        services.AddScoped<StreamingHtmlRenderer>();
        services.AddScoped<ConventionRouteComponentResolver>();

        // Register IRouteComponentResolver (fallback to convention-based)
        services.AddScoped<IRouteComponentResolver>(sp =>
            sp.GetRequiredService<ConventionRouteComponentResolver>());

        return services;
    }
}
