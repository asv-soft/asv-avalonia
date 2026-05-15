using Avalonia;
using Material.Icons;

namespace Asv.Avalonia;

public partial class Tag
{
    public static readonly StyledProperty<MaterialIconKind?> IconProperty =
        AvaloniaProperty.Register<Tag, MaterialIconKind?>(
            nameof(Icon),
            defaultValue: MaterialIconKind.Tag
        );

    public MaterialIconKind? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    private string? _key;

    public static readonly DirectProperty<Tag, string?> KeyProperty =
        AvaloniaProperty.RegisterDirect<Tag, string?>(nameof(Key), o => o.Key, (o, v) => o.Key = v);

    public string? Key
    {
        get => _key;
        set => SetAndRaise(KeyProperty, ref _key, value);
    }

    private string? _value;

    public static readonly DirectProperty<Tag, string?> ValueProperty =
        AvaloniaProperty.RegisterDirect<Tag, string?>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v
        );

    public string? Value
    {
        get => _value;
        set => SetAndRaise(ValueProperty, ref _value, value);
    }
}
