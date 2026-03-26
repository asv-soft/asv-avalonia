using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.GeoMap;

public partial class MapCanvas : Panel
{
    static MapCanvas()
    {
        AffectsParentArrange<MapCanvas>(LocationProperty);
        AffectsArrange<MapCanvas>(
            ProviderProperty,
            ZoomProperty,
            CenterMapProperty,
            MapRotationProperty
        );
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        foreach (Control child in Children)
        {
            child.Measure(availableSize);
        }

        return availableSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        foreach (Control child in Children)
        {
            ArrangeChild(child, finalSize);
        }

        return finalSize;
    }

    protected virtual void ArrangeChild(Control child, Size finalSize)
    {
        var point = GetLocation(child);
        var offsetH = GetCenterX(child);
        var offsetV = GetCenterY(child);

        if (Provider is null || point is null)
        {
            return;
        }

        var tileSize = Provider.TileSize;
        var projection = Provider.Projection;
        var centerScreen = new Point(finalSize.Width * 0.5, finalSize.Height * 0.5);
        var centerPixel = projection.Wgs84ToPixels(CenterMap, Zoom, tileSize);
        var offset = centerScreen - centerPixel;
        var anchorPoint = projection.Wgs84ToPixels(point.Value, Zoom, tileSize) + offset;

        if (MapRotation != 0)
        {
            anchorPoint = RotatePoint(anchorPoint, centerScreen, MapRotation);
        }

        var pos = new Point(
            anchorPoint.X - offsetH.CalculateOffset(child.DesiredSize.Width),
            anchorPoint.Y - offsetV.CalculateOffset(child.DesiredSize.Height)
        );

        SetRotation(child, MapRotation);
        child.Arrange(new Rect(pos, child.DesiredSize));
    }

    private static Point RotatePoint(Point point, Point center, double angle)
    {
        var radians = angle * Math.PI / 180.0;
        var cosTheta = Math.Cos(radians);
        var sinTheta = Math.Sin(radians);
        var dx = point.X - center.X;
        var dy = point.Y - center.Y;

        return new Point(
            (dx * cosTheta) - (dy * sinTheta) + center.X,
            (dx * sinTheta) + (dy * cosTheta) + center.Y
        );
    }
}
