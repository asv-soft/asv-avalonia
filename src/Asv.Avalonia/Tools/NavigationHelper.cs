using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using R3;

namespace Asv.Avalonia;

public class NavigationHelper
{
    public static readonly AttachedProperty<bool> IsSelectedProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>(
            "IsSelected",
            typeof(NavigationHelper),
            defaultBindingMode: BindingMode.TwoWay
        );

    public static bool GetIsSelected(Control control) => control.GetValue(IsSelectedProperty);

    public static void SetIsSelected(Control control, bool value) =>
        control.SetValue(IsSelectedProperty, value);

    static NavigationHelper()
    {
        IsSelectedProperty
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


