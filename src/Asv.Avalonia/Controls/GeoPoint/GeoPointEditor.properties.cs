using Avalonia;

namespace Asv.Avalonia;

public partial class GeoPointEditor
{
    private string? _latitude;
    private string? _longitude;
    private string? _altitude;
    
    public static readonly StyledProperty<bool> IsReadOnlyProperty = AvaloniaProperty.Register<
        GeoPointEditor,
        bool
    >(nameof(IsReadOnly));

    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public static readonly DirectProperty<GeoPointEditor, string?> LatitudeProperty =
        AvaloniaProperty.RegisterDirect<GeoPointEditor, string?>(
            nameof(Latitude),
            o => o.Latitude,
            (o, v) => o.Latitude = v
        );

    public string? Latitude
    {
        get => _latitude;
        set => SetAndRaise(LatitudeProperty, ref _latitude, value);
    }

    public static readonly DirectProperty<GeoPointEditor, string?> LongitudeProperty =
        AvaloniaProperty.RegisterDirect<GeoPointEditor, string?>(
            nameof(Longitude),
            o => o.Longitude,
            (o, v) => o.Longitude = v
        );

    public string? Longitude
    {
        get => _longitude;
        set => SetAndRaise(LongitudeProperty, ref _longitude, value);
    }

    public static readonly DirectProperty<GeoPointEditor, string?> AltitudeProperty =
        AvaloniaProperty.RegisterDirect<GeoPointEditor, string?>(
            nameof(Altitude),
            o => o.Altitude,
            (o, v) => o.Altitude = v
        );

    public string? Altitude
    {
        get => _altitude;
        set => SetAndRaise(AltitudeProperty, ref _altitude, value);
    }
}