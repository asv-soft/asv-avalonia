using System.Collections.Immutable;

namespace Asv.Avalonia;

public interface IRoutable : IViewModel
{
    IRoutable? Parent { get; set; }
    IEnumerable<IRoutable> Children { get; }
    ValueTask Rise(AsyncRoutedEvent e);
    ValueTask<IRoutable> NavigateTo(ArraySegment<string> path);
}

public enum RoutingEventStrategy
{
    Bubble,
    Tunnel,
    Direct,
}

public abstract class AsyncRoutedEvent(IRoutable source, RoutingEventStrategy routingEventStrategy)
{
    public IRoutable Source { get; } = source;
    public RoutingEventStrategy RoutingEventStrategy { get; } = routingEventStrategy;
    public bool IsHandled { get; set; }

    public virtual AsyncRoutedEvent Clone()
    {
        return (AsyncRoutedEvent)MemberwiseClone();
    }
}
