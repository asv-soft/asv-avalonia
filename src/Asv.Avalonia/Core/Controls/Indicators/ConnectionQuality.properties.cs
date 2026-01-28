using Avalonia;
using Material.Icons;

namespace Asv.Avalonia;

public partial class ConnectionQuality
{
    public static readonly StyledProperty<double> CriticalValueProperty = AvaloniaProperty.Register<
        ConnectionQuality,
        double
    >(nameof(CriticalValue), 0.2);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }

    public static readonly StyledProperty<double> WarningValueProperty = AvaloniaProperty.Register<
        ConnectionQuality,
        double
    >(nameof(WarningValue), 0.5);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }

    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<
        ConnectionQuality,
        double
    >(nameof(MaxValue), 1);

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<
        ConnectionQuality,
        double?
    >(nameof(Value), default(double?));

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> IconKindProperty =
        AvaloniaProperty.Register<ConnectionQuality, MaterialIconKind>(
            nameof(IconKind),
            MaterialIconKind.WifiStrengthAlertOutline
        );

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
}
