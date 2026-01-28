using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using R3;

namespace Asv.Avalonia;

public class NavigationHelper
{
    public static readonly AttachedProperty<bool> IsFocusedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsFocused",
            typeof(NavigationHelper),
            defaultBindingMode: BindingMode.TwoWay
        );

    public static bool GetIsFocused(Control control) => control.GetValue(IsFocusedProperty);

    public static void SetIsFocused(Control control, bool value) =>
        control.SetValue(IsFocusedProperty, value);

    static NavigationHelper()
    {
        IsFocusedProperty
            .Changed.ToObservable()
            .Subscribe(args =>
            {
                if (args.Sender is Control control && args.NewValue.GetValueOrDefault())
                {
                    control.Focus();
                }
            });
    }
}
