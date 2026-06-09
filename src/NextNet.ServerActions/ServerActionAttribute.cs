namespace NextNet.ServerActions;

/// <summary>
/// Marks a class or static method as a server action.
/// Server actions are automatically exposed as HTTP POST endpoints at <c>/_actions/{name}</c>
/// and can be invoked from client code via a generated proxy.
/// </summary>
/// <example>
/// Marking a static method as a server action:
/// <code>
/// public static class MyActions
/// {
///     [ServerAction]
///     public static string Hello(string name) => $"Hello, {name}!";
/// }
/// </code>
/// Marking a class so all its public methods become actions:
/// <code>
/// [ServerAction]
/// public static class UserActions
/// {
///     public static object GetUser(int id) => ...;
///     public static object CreateUser(string name) => ...;
/// }
/// </code>
/// Specifying a custom name and requiring authentication:
/// <code>
/// [ServerAction(Name = "delete-user", RequireAuth = true)]
/// public static object DeleteUser(int id) => ...;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ServerActionAttribute : Attribute
{
    /// <summary>
    /// Optional override for the action name used in the route and client proxy.
    /// When not set, the method name is used.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional override for the route template. When not set,
    /// the route defaults to <c>/_actions/{name}</c>.
    /// </summary>
    public string? Route { get; set; }

    /// <summary>
    /// When <c>true</c>, the action requires authentication.
    /// Default: <c>false</c> (anonymous access allowed).
    /// </summary>
    public bool RequireAuth { get; set; } = false;
}
