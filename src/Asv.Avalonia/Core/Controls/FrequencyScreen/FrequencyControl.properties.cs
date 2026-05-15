using Avalonia;

namespace Asv.Avalonia;

public partial class FrequencyControl
{
    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<
        FrequencyControl,
        string?
    >(nameof(Value));

    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
}
