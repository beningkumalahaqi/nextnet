namespace NextNet.DevTools;

/// <summary>
/// Event bus interface for DevTools publish/subscribe communication.
/// Enables loose coupling between DevTools components (panels, data store, server).
/// </summary>
/// <example>
/// <code>
/// // Publishing
/// eventBus.Publish(new RouteDiscoveredEvent { Path = "/", Type = "static", File = "app/page.cs" });
///
/// // Subscribing
/// using var sub = eventBus.Subscribe&lt;RouteDiscoveredEvent&gt;(evt =>
/// {
///     Console.WriteLine($"Route discovered: {evt.Path}");
/// });
/// </code>
/// </example>
public interface IDevToolsEventBus
{
    /// <summary>
    /// Publish an event to all subscribers of type <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event type (must implement <see cref="IDevToolsEvent"/>).</typeparam>
    /// <param name="evt">The event payload.</param>
    void Publish<TEvent>(TEvent evt) where TEvent : IDevToolsEvent;

    /// <summary>
    /// Subscribe to an event type. Returns an <see cref="IDisposable"/> that unsubscribes when disposed.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="handler">The handler to invoke when the event is published.</param>
    /// <returns>A disposable token. Dispose to unsubscribe.</returns>
    IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDevToolsEvent;
}
