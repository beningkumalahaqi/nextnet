namespace NextNet.Templates.Official;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextNet.Templates.Abstractions;
using NextNet.Templates.Official.Api;
using NextNet.Templates.Official.Blog;
using NextNet.Templates.Official.Dashboard;
using NextNet.Templates.Official.Saas;

/// <summary>
/// Provides dependency injection extension methods for registering NextNet official
/// template services with an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="AddNextNetOfficialTemplates"/> in your application's startup code
/// to register all official template providers, including the Blog, API, and Dashboard
/// template providers.
/// </para>
/// <example>
/// <code>
/// var services = new ServiceCollection();
/// services.AddNextNetOfficialTemplates();
///
/// var provider = services.BuildServiceProvider();
/// var blogProvider = provider.GetRequiredService&lt;BlogTemplateProvider&gt;();
/// var apiProvider = provider.GetRequiredService&lt;ApiTemplateProvider&gt;();
/// var dashboardProvider = provider.GetRequiredService&lt;DashboardTemplateProvider&gt;();
/// </code>
/// </example>
/// </remarks>
public static class OfficialTemplatesServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet official template providers to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetOfficialTemplates();
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetOfficialTemplates(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<BlogTemplateProvider>();
        services.TryAddSingleton<ApiTemplateProvider>();
        services.TryAddSingleton<DashboardTemplateProvider>();

        // Register all providers under ITemplateProvider
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITemplateProvider, BlogTemplateProvider>(sp => sp.GetRequiredService<BlogTemplateProvider>()));
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITemplateProvider, ApiTemplateProvider>(sp => sp.GetRequiredService<ApiTemplateProvider>()));

        services.TryAddSingleton<SaasTemplateProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<ITemplateProvider, SaasTemplateProvider>(sp => sp.GetRequiredService<SaasTemplateProvider>()));

        return services;
    }
}
