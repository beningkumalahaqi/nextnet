using Microsoft.Extensions.DependencyInjection;
using NextNet.Edge.Adapters;
using NextNet.Edge.Build;
using NextNet.Edge.Compatibility;
using NextNet.Edge.Middleware;
using NextNet.Edge.Streaming;

namespace NextNet.Edge;

/// <summary>
/// Extension methods for registering NextNet Edge services with the DI container.
/// </summary>
public static class EdgeServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet Edge runtime services to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="options">Optional edge configuration. If null, defaults are used.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetEdge(
        this IServiceCollection services,
        EdgeOptions? options = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        options ??= new EdgeOptions();
        services.AddSingleton(options);

        // Register the adapter registry
        services.AddSingleton<AdapterRegistry>();

        // Register compatibility checker
        services.AddSingleton<EdgeCompatibilityChecker>();
        services.AddSingleton<EdgeApiWhitelist>();

        // Note: IEdgeStreamWriter is not registered in DI because it requires
        // a per-request response stream. Consumers should construct it manually:
        //   new EdgeStreamWriter(httpContext.Response.Body, options)
        // or use the EdgeStreamWriter(HttpResponse, EdgeOptions) constructor.

        // Register edge build services
        services.AddTransient<EdgeBuildStep>();
        services.AddTransient<EdgeEntryGenerator>();

        // Register adapters
        services.AddTransient<CloudflareWorkersAdapter>();
        services.AddTransient<VercelEdgeAdapter>();
        services.AddTransient<DenoDeployAdapter>();
        services.AddTransient<AwsLambdaEdgeAdapter>();

        return services;
    }

    /// <summary>
    /// Adds NextNet Edge runtime services, configured via the specified delegate.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">A delegate to configure <see cref="EdgeOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> or <paramref name="configure"/> is null.</exception>
    public static IServiceCollection AddNextNetEdge(
        this IServiceCollection services,
        Action<EdgeOptions> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var options = new EdgeOptions();
        configure(options);
        return services.AddNextNetEdge(options);
    }
}
