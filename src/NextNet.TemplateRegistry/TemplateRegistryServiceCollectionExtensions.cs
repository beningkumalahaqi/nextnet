using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextNet.Templates.Abstractions;

namespace NextNet.TemplateRegistry;

/// <summary>
/// Extension methods for registering the NextNet template registry services with DI.
/// </summary>
public static class TemplateRegistryServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="ITemplateRegistry"/> implementation and its supporting services
    /// (<see cref="HttpTemplateRegistryClient"/>, <see cref="TemplateRegistryCache"/>) into the
    /// service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configure">An optional delegate to configure <see cref="RegistryOptions"/>.</param>
    /// <returns>The same service collection for chaining.</returns>
    /// <example>
    /// <code>
    /// services.AddNextNetTemplateRegistry(options =>
    /// {
    ///     options.Url = "https://my-mirror.nextnet.dev";
    ///     options.CacheTtl = TimeSpan.FromMinutes(30);
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetTemplateRegistry(this IServiceCollection services, Action<RegistryOptions>? configure = null)
    {
        var options = new RegistryOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);

        services.AddHttpClient<HttpTemplateRegistryClient>();
        services.AddSingleton<TemplateRegistryCache>();
        services.AddSingleton<TemplateRegistry>();
        services.AddSingleton<ITemplateRegistry>(sp => sp.GetRequiredService<TemplateRegistry>());
        return services;
    }
}
