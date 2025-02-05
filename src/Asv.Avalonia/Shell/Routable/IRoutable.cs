using R3;

namespace Asv.Avalonia;

public interface IRoutable : IViewModel
{
    IRoutable? Parent { get; set; }
    ValueTask Rise(AsyncRoutedEvent e);
    ValueTask<IRoutable> Navigate(string id);
    IEnumerable<IRoutable> GetRoutableChildren();
}

public abstract class AsyncRoutedEvent(
    IRoutable source,
    RoutingStrategy routingStrategy = RoutingStrategy.Bubble
)
{
    public RoutingStrategy RoutingStrategy { get; set; }
    public IRoutable Source { get; } = source;
    public bool IsHandled { get; set; }

    public virtual AsyncRoutedEvent Clone()
    {
        return (AsyncRoutedEvent)MemberwiseClone();
    }
}

public enum RoutingStrategy
{
    Bubble,
    Tunnel,
    Direct,
}
