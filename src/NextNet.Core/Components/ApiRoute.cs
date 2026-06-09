using Microsoft.AspNetCore.Http;
using NextNet.Errors;

namespace NextNet.Components;

/// <summary>
/// Base class for API route handlers discovered from <c>app/api/**/route.cs</c> files.
/// Provides HTTP method virtuals (Get, Post, Put, Patch, Delete) that subclasses override.
/// Backward-compatible: the <see cref="Handle"/> method is still called by default for
/// any method that is not overridden.
/// </summary>
/// <example>
/// <code>
/// // app/api/hello/route.cs
/// public class HelloRoute : ApiRoute
/// {
///     public override Task&lt;IResult&gt; Get()
///         => Task.FromResult(Results.Ok(new { message = "Hello!" }));
///
///     public override Task&lt;IResult&gt; Handle(HttpContext context)
///         => Task.FromResult(Results.Ok("fallback"));
/// }
/// </code>
/// </example>
public abstract class ApiRoute
{
    /// <summary>
    /// Gets or sets the <see cref="Microsoft.AspNetCore.Http.HttpContext"/> for the current request.
    /// Set automatically by the generated endpoint handler before dispatching.
    /// </summary>
    public HttpContext? HttpContext { get; set; }

    /// <summary>
    /// Backward-compatible handler. Called by default when a specific HTTP method
    /// virtual is not overridden.
    /// </summary>
    /// <param name="context">The ASP.NET Core HTTP context.</param>
    /// <returns>An <see cref="IResult"/> representing the HTTP response.</returns>
    public abstract Task<IResult> Handle(HttpContext context);

    /// <summary>
    /// Handles HTTP GET requests. Default behavior delegates to <see cref="Handle"/>.
    /// Override to provide GET-specific logic.
    /// </summary>
    public virtual Task<IResult> Get()
        => Handle(HttpContext ?? throw new InvalidOperationException($"[{CoreErrorCodes.HttpContextNotSet}] HttpContext is not set."));

    /// <summary>
    /// Handles HTTP POST requests. Default behavior delegates to <see cref="Handle"/>.
    /// Override to provide POST-specific logic.
    /// </summary>
    public virtual Task<IResult> Post()
        => Handle(HttpContext ?? throw new InvalidOperationException($"[{CoreErrorCodes.HttpContextNotSet}] HttpContext is not set."));

    /// <summary>
    /// Handles HTTP PUT requests. Default behavior delegates to <see cref="Handle"/>.
    /// Override to provide PUT-specific logic.
    /// </summary>
    public virtual Task<IResult> Put()
        => Handle(HttpContext ?? throw new InvalidOperationException($"[{CoreErrorCodes.HttpContextNotSet}] HttpContext is not set."));

    /// <summary>
    /// Handles HTTP PATCH requests. Default behavior delegates to <see cref="Handle"/>.
    /// Override to provide PATCH-specific logic.
    /// </summary>
    public virtual Task<IResult> Patch()
        => Handle(HttpContext ?? throw new InvalidOperationException($"[{CoreErrorCodes.HttpContextNotSet}] HttpContext is not set."));

    /// <summary>
    /// Handles HTTP DELETE requests. Default behavior delegates to <see cref="Handle"/>.
    /// Override to provide DELETE-specific logic.
    /// </summary>
    public virtual Task<IResult> Delete()
        => Handle(HttpContext ?? throw new InvalidOperationException($"[{CoreErrorCodes.HttpContextNotSet}] HttpContext is not set."));
}
