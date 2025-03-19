using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Avalonia;

public partial class SettingsKeymapView
{
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