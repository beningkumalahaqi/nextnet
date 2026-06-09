using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Logging;
using NextNet.Rendering;
using NextNet.UI.Rendering.Head;
using NextNet.UI.Rendering.Middleware;

namespace NextNet.UI.Rendering.Extensions;

/// <summary>
/// Extension methods for registering NextNet UI Rendering middleware in the
/// ASP.NET Core request pipeline.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ApplicationBuilderExtensions"/> provides the <c>UseNextNetUi</c>
/// extension method that registers the <see cref="UiSsrMiddleware"/> in the
/// middleware pipeline. This middleware extends the standard SSR rendering with
/// theme CSS injection into the HTML <c>&lt;head&gt;</c>.
/// </para>
/// <para>
/// Call <c>UseNextNetUi()</c> after static file middleware and before
/// endpoint routing to ensure UI theming is applied to all rendered pages.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // In Program.cs:
/// var app = builder.Build();
/// app.UseStaticFiles();
/// app.UseNextNetUi();
/// app.MapRazorPages(); // or app.UseEndpoints(...)
/// app.Run();
/// </code>
/// </example>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the NextNet UI rendering middleware to the request pipeline.
    /// This middleware injects theme CSS into the page head before rendering
    /// and extends the standard SSR pipeline with UI theming support.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is <c>null</c>.</exception>
    public static IApplicationBuilder UseNextNetUi(this IApplicationBuilder app)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));

        var logger = app.ApplicationServices.GetService<INextNetLogger>();
        var ssrRenderer = app.ApplicationServices.GetRequiredService<SsrRenderer>();
        var themeInjector = app.ApplicationServices.GetService<ThemeHeadInjector>();

        app.UseMiddleware<UiSsrMiddleware>(ssrRenderer, themeInjector, logger);

        return app;
    }

    /// <summary>
    /// Adds the NextNet UI rendering middleware to the request pipeline with
    /// the specified theme name as the default.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
    /// <param name="defaultTheme">The default theme name to use (e.g., "light", "dark").</param>
    /// <returns>The same application builder for chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="defaultTheme"/> is null or empty.</exception>
    public static IApplicationBuilder UseNextNetUi(
        this IApplicationBuilder app,
        string defaultTheme)
    {
        if (app == null) throw new ArgumentNullException(nameof(app));
        if (string.IsNullOrWhiteSpace(defaultTheme))
            throw new ArgumentException("Default theme name cannot be null or empty.", nameof(defaultTheme));

        // Store default theme in application services
        var options = app.ApplicationServices.GetService<UiRenderingOptions>();
        if (options != null)
        {
            options.DefaultTheme = defaultTheme;
        }

        return UseNextNetUi(app);
    }
}
