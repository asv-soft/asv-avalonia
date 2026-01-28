using Avalonia;

namespace Asv.Avalonia.GeoMap;

public partial class PolygonLayer
{
    public static readonly StyledProperty<MapItemsControl?> SourceProperty =
        AvaloniaProperty.Register<PolygonLayer, MapItemsControl?>(nameof(Source));

    public MapItemsControl? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }
}
