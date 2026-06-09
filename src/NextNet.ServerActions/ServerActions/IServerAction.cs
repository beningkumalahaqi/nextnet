namespace NextNet.ServerActions.ServerActions;

/// <summary>
/// Optional interface for server action classes that require DI injection.
/// Implement this interface on a class marked with <see cref="ServerActionAttribute"/>
/// to receive dependency-injected services via the <see cref="SetServices"/> method
/// before action execution.
/// </summary>
/// <example>
/// <code>
/// [ServerAction]
/// public class UserActions : IServerAction
/// {
///     private IUserService? _userService;
///
///     public void SetServices(IServiceProvider services)
///     {
///         _userService = services.GetRequiredService&lt;IUserService&gt;();
///     }
///
///     public Task&lt;ActionResult&gt; GetUser(int id)
///     {
///         var user = _userService!.GetById(id);
///         return Task.FromResult(ActionSuccess.With(user));
///     }
/// }
/// </code>
/// </example>
public interface IServerAction
{
    /// <summary>
    /// Called by the action executor to set resolved DI services.
    /// Implementations should store services in fields/properties for use by action methods.
    /// </summary>
    /// <param name="services">The service provider for the current request scope.</param>
    void SetServices(IServiceProvider services);
}
