using Avalonia.Input;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface ICommandInfo
{
    string Id { get; init; }
    string Name { get; init; }
    string Description { get; init; }
    MaterialIconKind Icon { get; init; }
    IHotKeyInfo HotKeyInfo { get; init; }
    IExportInfo Source { get; init; }
}

public class CommandInfo : ICommandInfo
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required MaterialIconKind Icon { get; init; }
    public required IHotKeyInfo HotKeyInfo { get; init; }
    public required IExportInfo Source { get; init; }
}
