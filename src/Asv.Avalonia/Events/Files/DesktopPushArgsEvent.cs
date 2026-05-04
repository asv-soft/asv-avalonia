using Asv.Common;
using Asv.Modeling;

namespace Asv.Avalonia;

public class DesktopPushArgsEvent(IViewModel source, IAppArgs args)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public IAppArgs Args { get; } = args;
}
