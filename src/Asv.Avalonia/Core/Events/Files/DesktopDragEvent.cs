using Asv.Common;
using Asv.Modeling;
using Avalonia.Input;

namespace Asv.Avalonia;

public class DesktopDragEvent(IViewModel source, DragEventArgs args)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble)
{
    public DragEventArgs Args { get; } = args;
}
