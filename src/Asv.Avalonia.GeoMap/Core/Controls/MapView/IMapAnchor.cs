using Asv.Common;
using Avalonia.Media;
using ObservableCollections;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// Describes a map item that can be positioned, rendered as an icon, annotated, and optionally
/// used as a polyline or polygon source.
/// </summary>
public interface IMapAnchor : IHasIcon, IHasHeader
{
    /// <summary>
    /// Gets the tree view of <see cref="Menu"/> used by Avalonia bindings to render the anchor context menu.
    /// </summary>
    public MenuTree MenuView { get; }

    /// <summary>
    /// Gets the menu items used to build the anchor context menu.
    /// </summary>
    public ObservableList<IMenuItem> Menu { get; }

    /// <summary>
    /// Gets or sets the item rotation angle in degrees.
    /// </summary>
    double Azimuth { get; set; }

    /// <summary>
    /// Gets or sets the geographic position of the anchor.
    /// </summary>
    GeoPoint Location { get; set; }

    /// <summary>
    /// Gets or sets the rendered icon size in device-independent pixels.
    /// </summary>
    double IconSize { get; set; }

    /// <summary>
    /// Gets or sets the horizontal point of the icon that is attached to <see cref="Location"/>.
    /// </summary>
    HorizontalOffset CenterX { get; set; }

    /// <summary>
    /// Gets or sets the vertical point of the icon that is attached to <see cref="Location"/>.
    /// </summary>
    VerticalOffset CenterY { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether user interaction can move this anchor.
    /// </summary>
    bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the anchor can be dragged without holding a keyboard
    /// modifier.
    /// </summary>
    bool CanDragWithoutModifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the item rotation includes the current map rotation.
    /// </summary>
    bool UseMapRotation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the anchor is selected by the map control.
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the anchor is rendered.
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the pen used to render <see cref="Polygon"/>.
    /// </summary>
    IPen? PolygonPen { get; set; }

    /// <summary>
    /// Gets or sets the brush used to fill <see cref="Polygon"/> when it is closed.
    /// </summary>
    IBrush? PolygonFill { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether <see cref="Polygon"/> is rendered as a closed shape.
    /// </summary>
    bool IsPolygonClosed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the map annotation for this anchor is visible.
    /// </summary>
    bool IsAnnotationVisible { get; set; }

    /// <summary>
    /// Gets the geographic points used to render a polyline or polygon for this anchor.
    /// </summary>
    ObservableList<GeoPoint> Polygon { get; }

    /// <summary>
    /// Gets a synchronized view of <see cref="Polygon"/> used by Avalonia bindings.
    /// </summary>
    NotifyCollectionChangedSynchronizedViewList<GeoPoint> PolygonView { get; }
}
