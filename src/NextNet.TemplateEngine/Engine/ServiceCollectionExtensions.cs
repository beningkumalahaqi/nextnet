using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NextNet.IO;
using NextNet.Templates.Abstractions;

namespace NextNet.TemplateEngine;

/// <summary>
/// Provides dependency injection extension methods for registering NextNet Template Engine
/// services with an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>
/// <para>
/// Use <see cref="AddNextNetTemplateEngine"/> in your application's startup code to register
/// all template engine services, including <see cref="ISharpFileSystem"/>,
/// <see cref="TemplateEngine"/>, and <see cref="ITemplateEngine"/>.
/// </para>
/// <example>
/// <code>
/// var services = new ServiceCollection();
/// services.AddNextNetTemplateEngine();
///
/// var provider = services.BuildServiceProvider();
/// var engine = provider.GetRequiredService&lt;ITemplateEngine&gt;();
/// </code>
/// </example>
/// </remarks>
public static class TemplateEngineServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet Template Engine services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service collection so that multiple calls can be chained.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <example>
    /// <code>
    /// builder.Services.AddNextNetTemplateEngine();
    /// </code>
    /// </example>
    public static IServiceCollection AddNextNetTemplateEngine(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<ISharpFileSystem, DefaultSharpFileSystem>();
        services.TryAddSingleton<TemplateEngine>();
        services.TryAddTransient<ITemplateEngine>(sp => sp.GetRequiredService<TemplateEngine>());
        return services;
    }
}
