using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NextNet.Middleware.Attributes;

namespace NextNet.Middleware;

/// <summary>
/// Represents a middleware registration within the pipeline.
/// </summary>
public class MiddlewareRegistration
{
    public Type? MiddlewareType { get; }
    public int Order { get; }
    public Func<HttpContext, bool>? Predicate { get; }
    public MiddlewarePipeline? Branch { get; }
    internal IMiddleware? InstanceOverride { get; }
    public string[]? RoutePatterns { get; }

    public MiddlewareRegistration(
        Type? middlewareType,
        int order,
        Func<HttpContext, bool>? predicate = null,
        MiddlewarePipeline? branch = null,
        IMiddleware? instanceOverride = null,
        string[]? routePatterns = null)
    {
        MiddlewareType = middlewareType;
        Order = order;
        Predicate = predicate;
        Branch = branch;
        InstanceOverride = instanceOverride;
        RoutePatterns = routePatterns;
    }
}

/// <summary>
/// Builds and executes the NextNet middleware pipeline.
/// Supports priority-based ordering, conditional execution, and branching.
/// </summary>
public class MiddlewarePipeline
{
    private readonly List<MiddlewareRegistration> _entries = new();
    private RequestDelegate? _builtPipeline;
    private bool _isBuilt;
    private readonly object _lock = new();

    /// <summary>
    /// Gets the collection of middleware registrations for inspection.
    /// </summary>
    public IReadOnlyList<MiddlewareRegistration> Registrations => _entries.AsReadOnly();

    /// <summary>
    /// Registers a middleware type in the pipeline with its default order.
    /// The order is determined by <see cref="MiddlewareOrderAttribute"/> on the type,
    /// or defaults to <see cref="MiddlewareOrder.Normal"/>.
    /// </summary>
    /// <typeparam name="T">The middleware type implementing <see cref="IMiddleware"/>.</typeparam>
    public void Use<T>() where T : IMiddleware
    {
        Use(typeof(T), GetOrder(typeof(T)));
    }

    /// <summary>
    /// Registers a middleware type with a specific order.
    /// </summary>
    /// <typeparam name="T">The middleware type implementing <see cref="IMiddleware"/>.</typeparam>
    /// <param name="order">The execution order (lower runs first).</param>
    public void Use<T>(int order) where T : IMiddleware
    {
        Use(typeof(T), order);
    }

    /// <summary>
    /// Registers a middleware instance directly in the pipeline.
    /// Useful when middleware requires constructor arguments that are not resolved from DI.
    /// </summary>
    /// <param name="instance">The middleware instance.</param>
    /// <param name="order">The execution order (lower runs first).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="instance"/> is null.</exception>
    public void Use(IMiddleware instance, int order = MiddlewareOrder.Normal)
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));

        _entries.Add(new MiddlewareRegistration(instance.GetType(), order, instanceOverride: instance));
        Invalidate();
    }

    /// <summary>
    /// Registers a middleware type by its <see cref="Type"/>.
    /// </summary>
    /// <param name="middlewareType">The middleware type implementing <see cref="IMiddleware"/>.</param>
    /// <param name="order">Optional explicit order. If null, the order is determined by attribute or defaults to <see cref="MiddlewareOrder.Normal"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="middlewareType"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="middlewareType"/> does not implement <see cref="IMiddleware"/>.</exception>
    public void Use(Type middlewareType, int? order = null)
    {
        if (middlewareType == null) throw new ArgumentNullException(nameof(middlewareType));
        if (!typeof(IMiddleware).IsAssignableFrom(middlewareType))
            throw new ArgumentException($"Type '{middlewareType.FullName}' does not implement {nameof(IMiddleware)}.", nameof(middlewareType));

        var resolvedOrder = order ?? GetOrder(middlewareType);
        _entries.Add(new MiddlewareRegistration(middlewareType, resolvedOrder));
        Invalidate();
    }

    /// <summary>
    /// Registers a middleware type scoped to specific route patterns,
    /// as defined by <see cref="UseMiddlewareAttribute.Routes"/>.
    /// The middleware only executes when the request path matches one of the patterns.
    /// Wildcard patterns (e.g., "/admin/*") are supported.
    /// </summary>
    /// <param name="middlewareType">The middleware type implementing <see cref="IMiddleware"/>.</param>
    /// <param name="routePatterns">The route patterns to match (e.g., "/admin/*", "/dashboard").</param>
    /// <param name="order">Optional explicit order.</param>
    public void Use(Type middlewareType, string[]? routePatterns, int? order = null)
    {
        if (middlewareType == null) throw new ArgumentNullException(nameof(middlewareType));
        if (!typeof(IMiddleware).IsAssignableFrom(middlewareType))
            throw new ArgumentException($"Type '{middlewareType.FullName}' does not implement {nameof(IMiddleware)}.", nameof(middlewareType));

        var resolvedOrder = order ?? GetOrder(middlewareType);
        var predicate = routePatterns is { Length: > 0 }
            ? CreateRoutePredicate(routePatterns)
            : null;
        _entries.Add(new MiddlewareRegistration(middlewareType, resolvedOrder, predicate, routePatterns: routePatterns));
        Invalidate();
    }

    /// <summary>
    /// Registers a middleware type that executes only when the given predicate returns true.
    /// </summary>
    /// <typeparam name="T">The middleware type implementing <see cref="IMiddleware"/>.</typeparam>
    /// <param name="predicate">A function that determines whether the middleware should execute.</param>
    /// <param name="order">Optional explicit order. If null, the order is determined by attribute or defaults to <see cref="MiddlewareOrder.Normal"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> is null.</exception>
    public void UseWhen<T>(Func<HttpContext, bool> predicate, int? order = null)
        where T : IMiddleware
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));

        var resolvedOrder = order ?? GetOrder(typeof(T));
        _entries.Add(new MiddlewareRegistration(typeof(T), resolvedOrder, predicate));
        Invalidate();
    }

    /// <summary>
    /// Registers a branch middleware pipeline that executes only when the given predicate returns true.
    /// This allows running a sub-pipeline of middleware conditionally.
    /// </summary>
    /// <param name="predicate">A function that determines whether the branch should execute.</param>
    /// <param name="configure">A delegate to configure the branch pipeline.</param>
    /// <param name="order">Optional explicit order for the branch.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="predicate"/> or <paramref name="configure"/> is null.</exception>
    public void UseWhen(Func<HttpContext, bool> predicate, Action<MiddlewarePipeline> configure, int order = 0)
    {
        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        var branch = new MiddlewarePipeline();
        configure(branch);
        _entries.Add(new MiddlewareRegistration(null, order, predicate, branch));
        Invalidate();
    }

    /// <summary>
    /// Builds the middleware pipeline into a <see cref="RequestDelegate"/>.
    /// The pipeline is cached once built and invalidated when new middleware is registered.
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving middleware instances.</param>
    /// <returns>A <see cref="RequestDelegate"/> representing the composed pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    public RequestDelegate Build(IServiceProvider serviceProvider)
    {
        return Build(serviceProvider, null);
    }

    /// <summary>
    /// Builds the middleware pipeline into a <see cref="RequestDelegate"/> with
    /// a custom terminal delegate (e.g., the next ASP.NET Core middleware).
    /// </summary>
    /// <param name="serviceProvider">The service provider for resolving middleware instances.</param>
    /// <param name="terminalDelegate">The final delegate to invoke after all middleware. If null, an empty no-op delegate is used.</param>
    /// <returns>A <see cref="RequestDelegate"/> representing the composed pipeline.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="serviceProvider"/> is null.</exception>
    public RequestDelegate Build(IServiceProvider serviceProvider, RequestDelegate? terminalDelegate)
    {
        if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));

        if (terminalDelegate == null && _isBuilt && _builtPipeline != null)
            return _builtPipeline;

        lock (_lock)
        {
            if (terminalDelegate == null && _isBuilt && _builtPipeline != null)
                return _builtPipeline;

            var sorted = _entries
                .OrderBy(e => e.Order)
                .ThenBy(e => e.MiddlewareType?.FullName ?? "")
                .ToList();

            // Terminal delegate — end of the pipeline
            RequestDelegate pipeline = terminalDelegate ?? (context => Task.CompletedTask);

            // Build in reverse so the first registered middleware wraps the last
            // (lowest order = outermost wrapper = runs first)
            for (int i = sorted.Count - 1; i >= 0; i--)
            {
                var entry = sorted[i];
                var next = pipeline;

                pipeline = async context =>
                {
                    // Skip if predicate fails
                    if (entry.Predicate != null && !entry.Predicate(context))
                    {
                        await next(context);
                        return;
                    }

                    // Execute branch pipeline
                    if (entry.Branch != null)
                    {
                        var branchPipeline = entry.Branch.Build(serviceProvider, terminalDelegate);
                        await branchPipeline(context);
                        return;
                    }

                    // Execute middleware with a shared MiddlewareContext per request
                    if (entry.MiddlewareType != null)
                    {
                        var middleware = entry.InstanceOverride ?? ResolveMiddleware(entry.MiddlewareType, serviceProvider);
                        var ctx = GetOrCreateContext(context);
                        await middleware.InvokeAsync(ctx, next);
                    }
                };
            }

            if (terminalDelegate == null)
            {
                _builtPipeline = pipeline;
                _isBuilt = true;
            }
            return pipeline;
        }
    }

    private MiddlewareContext GetOrCreateContext(HttpContext httpContext)
    {
        const string contextKey = "NextNet.Middleware.MiddlewareContext";
        if (httpContext.Items.TryGetValue(contextKey, out var existing) && existing is MiddlewareContext ctx)
            return ctx;

        ctx = new MiddlewareContext(httpContext, this);
        httpContext.Items[contextKey] = ctx;
        return ctx;
    }

    /// <summary>
    /// Creates a copy of the current pipeline configuration.
    /// The copy shares no state with the original and must be built separately.
    /// </summary>
    public MiddlewarePipeline Clone()
    {
        var clone = new MiddlewarePipeline();
        clone._entries.AddRange(_entries);
        return clone;
    }

    private void Invalidate()
    {
        _isBuilt = false;
        _builtPipeline = null;
    }

    private static int GetOrder(Type type)
    {
        var attr = type.GetCustomAttribute<MiddlewareOrderAttribute>(inherit: true);
        if (attr != null)
            return attr.Order;

        return MiddlewareOrder.Normal;
    }

    private static IMiddleware ResolveMiddleware(Type type, IServiceProvider serviceProvider)
    {
        // Try to resolve from DI first
        var instance = serviceProvider.GetService(type);
        if (instance != null)
            return (IMiddleware)instance;

        // Fall back to creating with DI-resolved dependencies
        return (IMiddleware)ActivatorUtilities.CreateInstance(serviceProvider, type);
    }

    /// <summary>
    /// Creates a predicate function that returns <c>true</c> when the request path
    /// matches any of the given route patterns. Supports wildcard patterns using
    /// <c>*</c> (e.g., <c>"/admin/*"</c> matches <c>"/admin/users"</c>).
    /// </summary>
    private static Func<HttpContext, bool> CreateRoutePredicate(string[] routePatterns)
    {
        // Convert each pattern to a compiled predicate for fast matching
        var matchers = routePatterns
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(CompileRoutePattern)
            .ToList();

        return context =>
        {
            var path = context.Request.Path.Value ?? string.Empty;
            foreach (var matcher in matchers)
            {
                if (matcher(path))
                    return true;
            }
            return false;
        };
    }

    /// <summary>
    /// Compiles a single route pattern (with optional trailing <c>*</c> wildcard)
    /// into a matching function.
    /// </summary>
    private static Func<string, bool> CompileRoutePattern(string pattern)
    {
        // Normalize: ensure leading /
        if (!pattern.StartsWith('/'))
            pattern = "/" + pattern;

        if (pattern.EndsWith('*'))
        {
            // Wildcard: match prefix
            var prefix = pattern[..^1]; // remove trailing *
            return path => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        // Exact match
        return path => string.Equals(path, pattern, StringComparison.OrdinalIgnoreCase);
    }
}
