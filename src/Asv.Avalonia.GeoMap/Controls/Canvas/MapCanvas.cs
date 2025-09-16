﻿using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia.GeoMap;

public partial class MapCanvas : Panel
{
    static MapCanvas()
    {
        AffectsParentArrange<MapCanvas>(LocationProperty);
        AffectsArrange<MapCanvas>(ProviderProperty, ZoomProperty, CenterMapProperty);
    }

    public MapCanvas()
    {
        Provider = new BingTileProvider(
            "Anqg-XzYo-sBPlzOWFHIcjC3F8s17P_O7L4RrevsHVg4fJk6g_eEmUBphtSn4ySg"
        );
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

        foreach (Control child in Children)
        {
            child.Measure(availableSize);
        }

        return default;
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

        if (Provider == null || point == null)
        {
            return;
        }

        var tileSize = Provider.TileSize;
        var halfWidth = Bounds.Width * 0.5;
        var halfHeight = Bounds.Height * 0.5;
        var projection = Provider.Projection;

        var centerPixel = projection.Wgs84ToPixels(CenterMap, Zoom, tileSize);
        var offset = new Point(halfWidth - centerPixel.X, halfHeight - centerPixel.Y);
        var pos = projection.Wgs84ToPixels(point.Value, Zoom, tileSize);

        pos += offset;
        pos = new Point(
            pos.X - offsetH.CalculateOffset(child.Bounds.Width),
            pos.Y - offsetV.CalculateOffset(child.Bounds.Height)
        );

        child.Arrange(new Rect(pos, child.DesiredSize));
    }
}
