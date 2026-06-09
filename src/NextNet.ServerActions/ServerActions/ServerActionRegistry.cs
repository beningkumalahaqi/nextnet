using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace NextNet.ServerActions.ServerActions;

/// <summary>
/// Registry of discovered server action methods.
/// Stores metadata about each action and caches invocation delegates for performance.
/// </summary>
/// <example>
/// Register actions and look them up by name:
/// <code>
/// var registry = new ServerActionRegistry();
/// registry.RegisterFromType(typeof(MyActions));
/// if (registry.TryGetAction("hello", out var descriptor))
/// {
///     var allActions = registry.GetAllActions();
///     Console.WriteLine($"Total: {registry.Count}");
/// }
/// </code>
/// </example>
public sealed class ServerActionRegistry
{
    private readonly ConcurrentDictionary<string, ServerActionDescriptor> _actions = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers all server actions from the specified assembly.
    /// Scans for types and methods marked with <see cref="ServerActionAttribute"/>.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The number of actions registered.</returns>
    public int RegisterFromAssembly(Assembly assembly)
    {
        if (assembly == null)
            throw new ArgumentNullException(nameof(assembly));

        var count = 0;

        foreach (var type in assembly.GetExportedTypes())
        {
            count += RegisterFromType(type);
        }

        return count;
    }

    /// <summary>
    /// Registers server actions from the specified type.
    /// If the type itself has <see cref="ServerActionAttribute"/>, all its public static methods are candidates.
    /// Individual methods can also be marked with the attribute.
    /// </summary>
    /// <param name="type">The type to scan.</param>
    /// <returns>The number of actions registered.</returns>
    public int RegisterFromType(Type type)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));

        var count = 0;
        var typeAttr = type.GetCustomAttribute<ServerActionAttribute>();
        var isActionClass = typeAttr != null;

        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
        {
            var methodAttr = method.GetCustomAttribute<ServerActionAttribute>();
            if (methodAttr == null && !isActionClass)
                continue;

            var attr = methodAttr ?? typeAttr!;
            var actionName = GetActionName(type, method, attr);

            if (_actions.ContainsKey(actionName))
                continue;

            var descriptor = new ServerActionDescriptor
            {
                ActionName = actionName,
                MethodInfo = method,
                DeclaringType = type,
                IsStatic = method.IsStatic,
                RequireAuth = attr.RequireAuth,
                Route = attr.Route ?? $"/_actions/{actionName}",
                Parameters = method.GetParameters()
                    .Select(p => new ActionParameterDescriptor
                    {
                        Name = p.Name ?? "unknown",
                        ParameterType = p.ParameterType,
                        IsService = IsServiceType(p.ParameterType),
                        IsCancellationToken = p.ParameterType == typeof(CancellationToken)
                    })
                    .ToList()
            };

            _actions[actionName] = descriptor;
            count++;
        }

        return count;
    }

    /// <summary>
    /// Tries to get a registered action descriptor by name.
    /// </summary>
    public bool TryGetAction(string actionName, out ServerActionDescriptor? descriptor)
    {
        return _actions.TryGetValue(actionName, out descriptor);
    }

    /// <summary>
    /// Gets all registered action descriptors.
    /// </summary>
    public IReadOnlyCollection<ServerActionDescriptor> GetAllActions()
    {
        return _actions.Values.ToList().AsReadOnly();
    }

    /// <summary>
    /// Gets the count of registered actions.
    /// </summary>
    public int Count => _actions.Count;

    private static string GetActionName(Type type, MethodInfo method, ServerActionAttribute attr)
    {
        if (!string.IsNullOrWhiteSpace(attr.Name))
            return attr.Name!;

        return method.Name;
    }

    private static bool IsServiceType(Type type)
    {
        // Types that are typically resolved from DI rather than deserialized from the request
        if (type == typeof(HttpContext))
            return true;
        if (type == typeof(CancellationToken))
            return true;
        if (type.IsInterface || type.IsAbstract)
            return true;
        if (type.Name.EndsWith("Service", StringComparison.Ordinal) ||
            type.Name.EndsWith("Repository", StringComparison.Ordinal) ||
            type.Name.EndsWith("Provider", StringComparison.Ordinal) ||
            type.Name.EndsWith("Factory", StringComparison.Ordinal) ||
            type.Name.EndsWith("Manager", StringComparison.Ordinal))
            return true;

        return false;
    }

    /// <summary>
    /// Clears all registered actions.
    /// </summary>
    internal void Clear() => _actions.Clear();
}

/// <summary>
/// Describes a registered server action.
/// </summary>
/// <example>
/// Descriptors are created automatically by <see cref="ServerActionRegistry.RegisterFromType"/>.
/// <code>
/// if (registry.TryGetAction("createUser", out var descriptor))
/// {
///     Console.WriteLine($"Action: {descriptor.ActionName}, Route: {descriptor.Route}");
/// }
/// </code>
/// </example>
public sealed record ServerActionDescriptor
{
    /// <summary>
    /// The action name used for routing.
    /// </summary>
    public string ActionName { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="MethodInfo"/> for the action method.
    /// </summary>
    public MethodInfo MethodInfo { get; set; } = null!;

    /// <summary>
    /// The type that declares the action method.
    /// </summary>
    public Type DeclaringType { get; set; } = null!;

    /// <summary>
    /// Whether the method is static.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Whether the action requires authentication.
    /// </summary>
    public bool RequireAuth { get; set; }

    /// <summary>
    /// The route template for the action endpoint.
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// The action method's parameters.
    /// </summary>
    public List<ActionParameterDescriptor> Parameters { get; set; } = new();
}

/// <summary>
/// Describes a parameter of a server action method.
/// </summary>
/// <example>
/// <code>
/// foreach (var param in descriptor.Parameters)
/// {
///     Console.WriteLine($"{param.Name}: {param.ParameterType.Name}" +
///         $" (service={param.IsService}, cancel={param.IsCancellationToken})");
/// }
/// </code>
/// </example>
public sealed record ActionParameterDescriptor
{
    /// <summary>
    /// The parameter name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The parameter type.
    /// </summary>
    public Type ParameterType { get; set; } = null!;

    /// <summary>
    /// Whether the parameter should be resolved from DI rather than deserialized.
    /// </summary>
    public bool IsService { get; set; }

    /// <summary>
    /// Whether the parameter is a <see cref="CancellationToken"/>.
    /// </summary>
    public bool IsCancellationToken { get; set; }
}
