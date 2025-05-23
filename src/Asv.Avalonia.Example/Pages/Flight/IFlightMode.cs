using Asv.Avalonia.Map;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Example;

public interface IFlightMode : IPage
{
    ObservableList<IMapWidget> Widgets { get; }
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
}
