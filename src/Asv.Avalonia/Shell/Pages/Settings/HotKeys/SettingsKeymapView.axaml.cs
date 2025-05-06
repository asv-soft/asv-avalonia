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

    private Key _previousKey;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not SettingsKeymapViewModel vm)
        {
            return;
        }

        if (!vm.SelectedItem.Value.IsChangingHotKey.Value)
        {
            return;
        }
        
        var rawGesture = vm.SelectedItem.Value.NewHotKeyValue.Value ?? string.Empty;
        base.OnKeyDown(e);
        if (rawGesture.Length == 0)
        {
            _previousKey = default;
        }
        
        if ((rawGesture.Length == 0 && !IsModifierKey(e.Key))
            || (rawGesture != string.Empty && !rawGesture.EndsWith('+'))
            || (IsModifierKey(e.Key) && IsModifierKey(_previousKey))
           )
        {
            return;
        }

        var keyValue = $"{e.Key}";
        if (e.Key is Key.LWin or Key.LWin)
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
                _ => keyValue,
            };
            rawGesture += $"{keyValue}+";
        }
        else
        {
            rawGesture += $"{keyValue}";
        }

        _previousKey = e.Key;
        vm.SelectedItem.Value.NewHotKeyValue.Value = rawGesture;
    }

    private bool IsModifierKey(Key key)
    {
        return key
            is Key.LeftShift
            or Key.RightShift
            or Key.LeftCtrl
            or Key.RightCtrl
            or Key.LeftAlt
            or Key.LeftAlt;
    }
}