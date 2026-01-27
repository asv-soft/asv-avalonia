using Avalonia;

namespace Asv.Avalonia.Example;

public partial class BoolIndicator
{
    private const double DefaultIconSize = 16.0;

    public static readonly StyledProperty<bool?> ValueProperty = AvaloniaProperty.Register<
        BoolIndicator,
        bool?
    >(nameof(Value));

    public bool? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<double> IconSizeProperty = AvaloniaProperty.Register<
        BoolIndicator,
        double
    >(nameof(IconSize), defaultValue: DefaultIconSize);

    public double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }
}
