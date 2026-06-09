using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using NextNet.ServerActions;
using NextNet.ServerActions.Client;
using NextNet.ServerActions.Middleware;
using NextNet.ServerActions.Serialization;
using NextNet.ServerActions.ServerActions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering NextNet Server Actions services in the DI container.
/// </summary>
/// <example>
/// Register services in <c>Program.cs</c>:
/// <code>
/// builder.Services.AddNextNetServerActions(options =>
/// {
///     options.AutoDiscoverAssemblies = new[] { typeof(MyAction).Assembly };
///     options.EnableAntiForgery = true;
/// });
/// </code>
/// Add middleware to the pipeline:
/// <code>
/// app.UseNextNetServerActions();
/// </code>
/// </example>
public static class ServerActionServiceCollectionExtensions
{
    /// <summary>
    /// Adds NextNet Server Actions services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddNextNetServerActions(
        this IServiceCollection services,
        Action<ServerActionOptions>? configure = null)
    {
        if (services == null)
            throw new ArgumentNullException(nameof(services));

        // Register core services
        services.AddSingleton<ServerActionRegistry>();
        services.AddSingleton<ServerActionInvoker>();
        services.AddSingleton<ServerActionSerializer>();
        services.AddTransient<ServerActionExecutor>();

        // Register client proxy generator
        services.AddSingleton<ServerActionClientProxyGenerator>();

        // Allow configuration
        var options = new ServerActionOptions();
        configure?.Invoke(options);

        // Register IHttpContextAccessor for service resolution
        services.AddHttpContextAccessor();

        // Register anti-forgery services for token validation on /_actions/ endpoints
        if (options.EnableAntiForgery)
        {
            services.AddAntiforgery();
        }

        // Auto-discover actions from loaded assemblies if requested
        if (options.AutoDiscoverAssemblies != null)
        {
            var registry = new ServerActionRegistry();
            foreach (var assembly in options.AutoDiscoverAssemblies)
            {
                registry.RegisterFromAssembly(assembly);
            }

            // Replace the singleton in the container
            services.AddSingleton(registry);
        }

        return services;
    }

    /// <summary>
    /// Adds the NextNet Server Actions middleware to the application pipeline.
    /// Must be called after <c>UseRouting()</c>.
    /// Anti-forgery middleware runs before action execution to validate tokens
    /// (only when <see cref="ServerActionOptions.EnableAntiForgery"/> is true).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseNextNetServerActions(this IApplicationBuilder app)
    {
        if (app == null)
            throw new ArgumentNullException(nameof(app));

        // Anti-forgery validation runs before the action executor (if registered)
        if (app.ApplicationServices.GetService<IAntiforgery>() is not null)
        {
            app.UseMiddleware<AntiForgeryMiddleware>();
        }

        return app.UseMiddleware<ServerActionMiddleware>();
    }
}

/// <summary>
/// Configuration options for NextNet Server Actions.
/// </summary>
/// <example>
/// Configure options when registering services:
/// <code>
/// services.AddNextNetServerActions(options =>
/// {
///     options.AutoDiscoverAssemblies = new[] { typeof(MyAction).Assembly };
///     options.EnableAntiForgery = true;
/// });
/// </code>
/// </example>
public sealed record ServerActionOptions
{
    /// <summary>
    /// Assemblies to scan for server action discovery at startup.
    /// When set, actions are auto-discovered and registered.
    /// </summary>
    public System.Reflection.Assembly[]? AutoDiscoverAssemblies { get; set; }

    /// <summary>
    /// When true, anti-forgery token validation is enforced on all
    /// POST requests to <c>/_actions/</c> endpoints.
    /// The host application must include the anti-forgery token in requests
    /// (e.g., via a hidden form field or header).
    /// Defaults to <c>false</c>.
    /// </summary>
    public bool EnableAntiForgery { get; set; }
}
