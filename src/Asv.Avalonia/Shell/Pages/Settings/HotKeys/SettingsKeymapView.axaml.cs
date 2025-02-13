using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsKeymapViewModel))]
public partial class SettingsKeymapView : UserControl
{
    public SettingsKeymapView()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not SettingsKeymapViewModel vm)
        {
            return;
        }

        if (vm.SelectedItem.Value is null)
        {
            return;
        }

        if (!vm.SelectedItem.Value.IsChangingHotKey.Value)
        {
            return;
        }

        var rawGesture = string.Empty;
        base.OnKeyDown(e);

        if ((vm.SelectedItem.Value.NewHotKeyValue.Value is { Length: 0 } && !IsModifierKey(e.Key)) || vm.SelectedItem.Value.NewHotKeyValue.Value is { Length: 2 })
        {
            return;
        }

        var keyValue = $"{e.Key}";
        if (e.Key == Key.LWin || e.Key == Key.LWin)
        {
            return;
        }

        if (IsModifierKey(e.Key))
        {
            keyValue = e.Key switch
            {
                Key.LeftAlt or Key.LeftAlt => KeyModifiers.Alt.ToString(),
                Key.RightCtrl or Key.LeftCtrl => KeyModifiers.Control.ToString(),
                Key.LeftShift or Key.RightShift => KeyModifiers.Shift.ToString(),
                _ => keyValue
            };
            rawGesture += $"{keyValue}+";
        }
        else
        {
            rawGesture += $"{keyValue}";
        }

        vm.SelectedItem.Value.NewHotKeyValue.Value += rawGesture;
    }

    private bool IsModifierKey(Key key)
    {
        return key is Key.LeftShift or Key.RightShift
            or Key.LeftCtrl or Key.RightCtrl;
    }
}