namespace Asv.Avalonia;

public abstract class LayoutEventBase(
    IRoutable source,
    ILayoutService layoutService,
    RoutingStrategy routingStrategy
) : AsyncRoutedEvent(source, routingStrategy)
{
    public const RoutingStrategy DefaultRoutingStrategy = RoutingStrategy.Tunnel;
    public ILayoutService LayoutService => layoutService;
}
