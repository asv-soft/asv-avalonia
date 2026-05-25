using Asv.Common;
using Avalonia.Media;
using ObservableCollections;

namespace Asv.Avalonia.GeoMap;

public interface IMapAnchor : IHasIcon, IHasHeader
{
    double Azimuth { get; set; }
    GeoPoint Location { get; set; }
    double IconSize { get; set; }
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
    ObservableList<GeoPoint> Polygon { get; }
    NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }
}
