using Avalonia;
using Avalonia.Data;
using Avalonia.Input;

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

    public static readonly StyledProperty<KeyGesture?> HotKeyProperty = AvaloniaProperty.Register<
        HotKeyBox,
        KeyGesture?
    >(nameof(HotKey), defaultBindingMode: BindingMode.TwoWay);

    public KeyGesture? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }
}
