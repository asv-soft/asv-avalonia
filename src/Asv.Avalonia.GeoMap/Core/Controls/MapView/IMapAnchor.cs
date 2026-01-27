using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapAnchor : IRoutable
{
    MaterialIconKind Icon { get; }
    string Title { get; }
    double Azimuth { get; }
    BindableReactiveProperty<GeoPoint> ReactiveLocation { get; }
    GeoPoint Location { get; }
    double IconSize { get; }
    AsvColorKind IconColor { get; }
    HorizontalOffset CenterX { get; }
    VerticalOffset CenterY { get; }
    bool IsReadOnly { get; }
    bool IsSelected { get; }
    bool IsVisible { get; }
    IPen? PolygonPen { get; }
    bool IsPolygonClosed { get; }
    NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }
}
