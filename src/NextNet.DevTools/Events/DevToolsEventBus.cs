namespace NextNet.DevTools;

/// <summary>
/// Simple in-memory event bus for DevTools publish/subscribe.
/// Thread-safe and resilient — subscriber exceptions are swallowed to keep the bus alive.
/// </summary>
/// <example>
/// <code>
/// var bus = new DevToolsEventBus();
/// using var sub = bus.Subscribe&lt;RouteDiscoveredEvent&gt;(evt => Console.WriteLine(evt.Path));
/// bus.Publish(new RouteDiscoveredEvent { Path = "/", Type = "static", File = "app/page.cs" });
/// </code>
/// </example>
public sealed class DevToolsEventBus : IDevToolsEventBus
{
    private readonly object _lock = new();
    private readonly Dictionary<Type, List<Delegate>> _handlers = new();

    /// <inheritdoc />
    public void Publish<TEvent>(TEvent evt) where TEvent : IDevToolsEvent
    {
        List<Delegate>? handlers;
        lock (_lock)
        {
            if (!_handlers.TryGetValue(typeof(TEvent), out handlers))
                return;
            // Snapshot under lock
            handlers = new List<Delegate>(handlers);
        }

        foreach (var handler in handlers)
        {
            try
            {
                ((Action<TEvent>)handler)(evt);
            }
            catch
            {
                // Swallow subscriber exceptions to keep bus alive
            }
        }
    }

    /// <inheritdoc />
    public IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : IDevToolsEvent
    {
        lock (_lock)
        {
            var eventType = typeof(TEvent);
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<Delegate>();
            _handlers[eventType].Add(handler);
        }

        return new Subscription<TEvent>(this, handler);
    }

    private sealed class Subscription<TEvent> : IDisposable where TEvent : IDevToolsEvent
    {
        private readonly DevToolsEventBus _bus;
        private readonly Action<TEvent> _handler;
        private bool _disposed;

        public Subscription(DevToolsEventBus bus, Action<TEvent> handler)
        {
            _bus = bus;
            _handler = handler;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            lock (_bus._lock)
            {
                if (_bus._handlers.TryGetValue(typeof(TEvent), out var handlers))
                {
                    handlers.Remove(_handler);
                }
            }
        }
    }
}
