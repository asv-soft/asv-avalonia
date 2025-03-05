using Asv.Avalonia.Map;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public interface IFlightModeContext : IPage
{
    ObservableList<IMapWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}
