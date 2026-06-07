using Microsoft.Extensions.DependencyInjection;
using NextNet.Build.StaticGeneration;
using NextNet.IO;
using NextNet.Logging;

namespace NextNet.Build.Extensions;

/// <summary>
/// Extension methods for registering NextNet Build services in the DI container.
/// </summary>
public static class BuildServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet Build services to the service collection, including
    /// the SSG pipeline and all related services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional delegate to configure SSG options.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddNextNetBuild(
        this IServiceCollection services,
        Action<SsgOptions>? configure = null)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        // Configure SsgOptions
        var options = new SsgOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        // Register file system abstraction
        services.AddSingleton<ISharpFileSystem>(_ => new DefaultSharpFileSystem());

        // Register OutputWriter
        services.AddSingleton<OutputWriter>(sp =>
        {
            var opts = sp.GetRequiredService<SsgOptions>();
            var fs = sp.GetRequiredService<ISharpFileSystem>();
            return new OutputWriter(fs.GetFullPath(opts.OutputDirectory), fs);
        });

        // Register the build pipeline (other services like SsrRenderer, RouteScanner
        // must be registered separately by the application host)
        services.AddTransient<BuildPipeline>();

        return services;
    }
}
