using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Components;
using NextNet.Logging;
using NextNet.Rendering.Middleware;

namespace NextNet.Rendering.Extensions;

/// <summary>
/// Extension methods for registering NextNet SSR middleware in the ASP.NET Core pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds NextNet SSR middleware to the request pipeline.
    /// Registers the middleware that intercepts HTTP requests and renders
    /// NextNet pages using server-side rendering (SSR).
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is <c>null</c>.</exception>
    public static IApplicationBuilder UseNextNet(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        var logger = app.ApplicationServices.GetService<INextNetLogger>();
        var ssrOptions = app.ApplicationServices.GetService<SsrOptions>() ?? new SsrOptions();
        var ssrRenderer = app.ApplicationServices.GetRequiredService<SsrRenderer>();
        var streamingRenderer = app.ApplicationServices.GetService<Streaming.StreamingHtmlRenderer>();

        app.UseMiddleware<SsrMiddleware>(ssrRenderer, streamingRenderer, ssrOptions, logger);

        return app;
    }

    /// <summary>
    /// Adds NextNet SSR middleware to the request pipeline with custom options.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configureOptions">A delegate to configure SSR options.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static IApplicationBuilder UseNextNet(
        this IApplicationBuilder app,
        Action<SsrOptions> configureOptions)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));
        if (configureOptions == null) throw new ArgumentNullException(nameof(configureOptions));

        var options = app.ApplicationServices.GetService<SsrOptions>() ?? new SsrOptions();
        configureOptions(options);

        return UseNextNet(app);
    }
}
