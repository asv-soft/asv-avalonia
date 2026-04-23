using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

public abstract class LayoutEventBase(
    IViewModel source,
    ILayoutService layoutService,
    RoutingStrategy routingStrategy
) : AsyncRoutedEvent<IViewModel>(source, routingStrategy)
{
    public const RoutingStrategy DefaultRoutingStrategy = RoutingStrategy.Tunnel;
    public ILayoutService LayoutService => layoutService;
}
