using Asv.Modeling;

namespace Asv.Avalonia;

public class GoToEvent(IViewModel sender, NavPath path) 
    : AsyncRoutedEvent<IViewModel>(sender, RoutingStrategy.Bubble)
{
    public NavPath Path => path;
}