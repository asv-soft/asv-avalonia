namespace Asv.Avalonia;

public class DesktopPushArgsEvent(IRoutable source, IAppArgs args)
    : AsyncRoutedEvent(source, RoutingStrategy.Bubble)
{
    public IAppArgs Args { get; } = args;
}
