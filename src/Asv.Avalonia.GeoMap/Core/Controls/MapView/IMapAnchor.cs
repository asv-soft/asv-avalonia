using Asv.Common;
using Avalonia.Media;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapAnchor : IViewModel
{
    MaterialIconKind Icon { get; set; }
    string Title { get; set; }
    double Azimuth { get; set; }
    GeoPoint Location { get; set; }
    double IconSize { get; set; }
    AsvColorKind IconColor { get; set; }
    HorizontalOffset CenterX { get; set; }
    VerticalOffset CenterY { get; set; }
    bool IsReadOnly { get; set; }
    bool UseMapRotation { get; set; }
    bool IsSelected { get; set; }
    bool IsVisible { get; set; }
    IPen? PolygonPen { get; set; }
    IBrush? PolygonFill { get; set; }
    bool IsPolygonClosed { get; set; }
    bool IsAnnotationVisible { get; set; }
    NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }
}
