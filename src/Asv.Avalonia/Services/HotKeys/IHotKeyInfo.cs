using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public interface IHotKeyInfo
{
    string ActionId { get; }
    string Name { get; }
    string Description { get; }
    MaterialIconKind Icon { get; }
    KeyGesture DefaultHotKey { get; }
}