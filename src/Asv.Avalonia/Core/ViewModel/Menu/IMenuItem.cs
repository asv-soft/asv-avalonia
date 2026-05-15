using Asv.Modeling;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public interface IMenuItem : IActionViewModel
{
    NavId ParentId { get; }
    bool StaysOpenOnClick { get; }
    bool IsEnabled { get; }
    KeyGesture? HotKey { get; }
    MenuItemToggleType ToggleType { get; }
    bool IsChecked { get; }
    string? GroupName { get; }
}
