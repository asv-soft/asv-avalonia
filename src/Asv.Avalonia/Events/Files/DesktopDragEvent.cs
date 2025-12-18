using Asv.Common;
using Avalonia.Input;

namespace Asv.Avalonia;

public class DesktopDragEvent(IRoutable source, DragEventArgs args)
    : AsyncRoutedEvent<IRoutable>(source, RoutingStrategy.Bubble)
{
    public DragEventArgs Args { get; } = args;
}
