namespace NextNet.ServerActions.ServerActions;

/// <summary>
/// Optional interface for server action classes that require DI injection.
/// Implement this interface on a class marked with <see cref="ServerActionAttribute"/>
/// to receive dependency-injected services via the <see cref="SetServices"/> method
/// before action execution.
/// </summary>
public interface IServerAction
{
    /// <summary>
    /// Called by the action executor to set resolved DI services.
    /// Implementations should store services in fields/properties for use by action methods.
    /// </summary>
    /// <param name="services">The service provider for the current request scope.</param>
    void SetServices(IServiceProvider services);
}
