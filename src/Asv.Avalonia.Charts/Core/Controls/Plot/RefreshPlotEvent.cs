using Asv.Modeling;

namespace Asv.Avalonia.Charts;

public class RefreshPlotEvent(IViewModel source)
    : AsyncRoutedEvent<IViewModel>(source, RoutingStrategy.Bubble) { }
