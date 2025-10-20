namespace Asv.Avalonia;

public abstract class LayoutEventBase(
    IRoutable source,
    ILayoutService layoutService,
    RoutingStrategy routingStrategy
) : AsyncRoutedEvent(source, routingStrategy)
{
    public ILayoutService LayoutService => layoutService;
}
