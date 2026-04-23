using Asv.Common;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMap : IViewModel
{
    ObservableList<IMapAnchor> Anchors { get; }
    BindableReactiveProperty<IMapAnchor?> SelectedAnchor { get; }
    BindableReactiveProperty<double> Rotation { get; }
    BindableReactiveProperty<GeoPoint> CenterMap { get; }
    BindableReactiveProperty<int> Zoom { get; }
    IReadOnlyBindableReactiveProperty<int> MinZoom { get; }
    IReadOnlyBindableReactiveProperty<int> MaxZoom { get; }
}
