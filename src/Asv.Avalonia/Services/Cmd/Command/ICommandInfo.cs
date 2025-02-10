using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia;

public interface ICommandInfo
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    MaterialIconKind Icon { get; }
    public bool IsEditable { get;  }
    public KeyGesture? DefaultHotKey { get; }
    public KeyGesture? CustomHotKey { get; set; }
    int Order { get; }
}

public class CommandInfo : ICommandInfo
{
    public required string Id { get; set; }
    public required bool IsEditable { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required MaterialIconKind Icon { get; set; }
    public required KeyGesture? DefaultHotKey { get; set; }

    public KeyGesture? CustomHotKey { get; set; }

    public required int Order { get; set; }
}
