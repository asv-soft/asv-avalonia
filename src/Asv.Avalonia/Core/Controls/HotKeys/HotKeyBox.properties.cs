using Avalonia;
using Avalonia.Data;

namespace Asv.Avalonia;

public partial class HotKeyBox
{
    public static readonly StyledProperty<bool> AutoFocusProperty = AvaloniaProperty.Register<
        HotKeyBox,
        bool
    >(nameof(AutoFocus), defaultValue: true);

    public bool AutoFocus
    {
        get => GetValue(AutoFocusProperty);
        set => SetValue(AutoFocusProperty, value);
    }

    public static readonly StyledProperty<HotKeyInfo?> HotKeyProperty = AvaloniaProperty.Register<
        HotKeyBox,
        HotKeyInfo?
    >(nameof(HotKey), defaultBindingMode: BindingMode.TwoWay);

    public HotKeyInfo? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }
}
