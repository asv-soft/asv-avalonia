using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class SaveAllLayoutCommand : ContextCommand<IRoutable>
{
    #region Static

    public const string Id = $"{SaveLayoutCommand.Id}.all";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Save All Layout",
        Description = "Command that saves layout of the whole app to file",
        Icon = MaterialIconKind.ContentSaveAll,
        DefaultHotKey = "Ctrl+Alt+L",
        Source = SystemModule.Instance,
    };

    #endregion

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.RequestSaveAllLayoutToFile(cancel);
        return null;
    }
}
