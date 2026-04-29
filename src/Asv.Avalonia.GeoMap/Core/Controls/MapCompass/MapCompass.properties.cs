using Avalonia;
using Avalonia.Data;
using Avalonia.Input;

namespace Asv.Avalonia.GeoMap;

public partial class MapCompass
{
    #region Rotation

    public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<
        MapCompass,
        double
    >(nameof(Rotation), defaultBindingMode: BindingMode.TwoWay);

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    #endregion

    #region DeadZone

    public static readonly StyledProperty<double> DeadZoneProperty = AvaloniaProperty.Register<
        MapCompass,
        double
    >(nameof(DeadZone), 0);

    public double DeadZone
    {
        get => GetValue(DeadZoneProperty);
        set => SetValue(DeadZoneProperty, value);
    }

    #endregion

    #region EnableTouchpadGestures

    public static readonly StyledProperty<bool> EnableTouchpadGesturesProperty =
        AvaloniaProperty.Register<MapCompass, bool>(nameof(EnableTouchpadGestures), true);

    public bool EnableTouchpadGestures
    {
        get => GetValue(EnableTouchpadGesturesProperty);
        set => SetValue(EnableTouchpadGesturesProperty, value);
    }

    #endregion

    #region MouseRotationSensitivity

    public static readonly StyledProperty<double> MouseRotationSensitivityProperty =
        AvaloniaProperty.Register<MapCompass, double>(nameof(MouseRotationSensitivity), 1.0);

    public double MouseRotationSensitivity
    {
        get => GetValue(MouseRotationSensitivityProperty);
        set => SetValue(MouseRotationSensitivityProperty, Math.Abs(value));
    }

    #endregion

    #region TouchpadRotationSensitivity

    public static readonly StyledProperty<double> TouchpadRotationSensitivityProperty =
        AvaloniaProperty.Register<MapCompass, double>(nameof(TouchpadRotationSensitivity), 1.0);

    public double TouchpadRotationSensitivity
    {
        get => GetValue(TouchpadRotationSensitivityProperty);
        set => SetValue(TouchpadRotationSensitivityProperty, Math.Abs(value));
    }

    #endregion

    #region GestureSurface

    public static readonly StyledProperty<InputElement?> GestureSurfaceProperty =
        AvaloniaProperty.Register<MapCompass, InputElement?>(nameof(GestureSurface));

    public InputElement? GestureSurface
    {
        get => GetValue(GestureSurfaceProperty);
        set => SetValue(GestureSurfaceProperty, value);
    }

    #endregion
}
