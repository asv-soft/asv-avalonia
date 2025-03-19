using Avalonia;

namespace Asv.Avalonia;

public partial class BatteryIndicator
{
    public static readonly StyledProperty<double> CriticalValueProperty = AvaloniaProperty.Register<
        BatteryIndicator,
        double
    >(nameof(CriticalValue), 20);

    public double CriticalValue
    {
        get => GetValue(CriticalValueProperty);
        set => SetValue(CriticalValueProperty, value);
    }

    public static readonly StyledProperty<double> WarningValueProperty = AvaloniaProperty.Register<
        BatteryIndicator,
        double
    >(nameof(WarningValue), 50);

    public double WarningValue
    {
        get => GetValue(WarningValueProperty);
        set => SetValue(WarningValueProperty, value);
    }

    public static readonly StyledProperty<double> MaxValueProperty = AvaloniaProperty.Register<
        BatteryIndicator,
        double
    >(nameof(MaxValue), 100);

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public static readonly StyledProperty<double?> ValueProperty = AvaloniaProperty.Register<
        BatteryIndicator,
        double?
    >(nameof(Value), default(double?));

    public double? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}