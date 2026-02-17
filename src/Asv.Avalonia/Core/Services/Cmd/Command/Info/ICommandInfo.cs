using Material.Icons;

namespace Asv.Avalonia;

public interface ICommandInfo
{
    string Id { get; init; }
    string Name { get; init; }
    string Description { get; init; }
    MaterialIconKind Icon { get; init; }
    AsvColorKind IconColor { get; set; }
    HotKeyInfo? DefaultHotKey { get; init; }
}
