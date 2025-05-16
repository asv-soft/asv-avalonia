using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsCommandListViewModel))]
public partial class SettingsCommandListView : UserControl
{
    public SettingsCommandListView()
    {
        InitializeComponent();
    }

    private Key _previousKey;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (DataContext is not SettingsCommandListViewModel vm)
        {
            base.OnKeyDown(e);
            return;
        }

        if (vm.SelectedItem.Value is null)
        {
            base.OnKeyDown(e);
            return;
        }

        if (!vm.SelectedItem.Value.IsChangingHotKey.Value)
        {
            base.OnKeyDown(e);
            return;
        }

        var rawGesture = vm.SelectedItem.Value.CurrentHotKeyString.Value ?? string.Empty;
        if (rawGesture.Length == 0)
        {
            _previousKey = default;
        }

        if (
            (rawGesture.Length == 0 && !IsModifierKey(e.Key))
            || (rawGesture != string.Empty && !rawGesture.EndsWith('+'))
            || (IsModifierKey(e.Key) && IsModifierKey(_previousKey))
        )
        {
            return;
        }

        var keyValue = $"{e.Key}";
        if (e.Key is Key.LWin or Key.RWin)
        {
            return;
        }

        if (IsModifierKey(e.Key))
        {
            keyValue = e.Key switch
            {
                Key.LeftAlt or Key.RightAlt => nameof(KeyModifiers.Alt),
                Key.RightCtrl or Key.LeftCtrl => nameof(KeyModifiers.Control),
                Key.LeftShift or Key.RightShift => nameof(KeyModifiers.Shift),
                _ => keyValue,
            };
            rawGesture += $"{keyValue}+";
        }
        else
        {
            rawGesture += $"{keyValue}";
        }

        _previousKey = e.Key;
        vm.SelectedItem.Value.CurrentHotKeyString.Value = rawGesture;
        e.Handled = true;
        base.OnKeyDown(e);
    }

    private bool IsModifierKey(Key key)
    {
        return key
            is Key.LeftShift
                or Key.RightShift
                or Key.LeftCtrl
                or Key.RightCtrl
                or Key.LeftAlt
                or Key.RightAlt;
    }
}
