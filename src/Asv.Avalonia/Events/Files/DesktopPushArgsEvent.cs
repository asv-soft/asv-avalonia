using Asv.Common;

namespace Asv.Avalonia;

public class DesktopPushArgsEvent(IRoutable source, IAppArgs args)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public IAppArgs Args { get; } = args;
}
