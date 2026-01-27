using Material.Icons;

namespace Asv.Avalonia;

public class CommandInfo : ICommandInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required MaterialIconKind Icon { get; init; }
    public AsvColorKind IconColor { get; set; }
    public required HotKeyInfo? DefaultHotKey { get; init; }
    public required IExportInfo Source { get; init; }
}
