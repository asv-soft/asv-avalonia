using Avalonia.Input;
using Material.Icons;

namespace Asv.Avalonia.IO;

public class AddPortCommand : NoContextCommand
{
    public static ICommandInfo StaticInfo = new CommandInfo
    {
        Id = $"{BaseId}.port.add",
        Name = "Add new port",
        Description = "Add a new port to the device manager",
        Icon = MaterialIconKind.PlusNetworkOutline,
        DefaultHotKey = KeyGesture.Parse("Ctrl+Shift+P"),
        CustomHotKey = null,
        Source = IoModule.Instance,
    };

    public override ICommandInfo Info => StaticInfo;

    protected override ValueTask<IPersistable?> InternalExecute(
        IPersistable newValue,
        CancellationToken cancel
    )
    {
        throw new NotImplementedException();
    }
}
