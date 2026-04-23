using Material.Icons;

namespace Asv.Avalonia;

public sealed class SaveAllLayoutToFileCommand : ContextCommand<IViewModel>
{
    #region Static

    public const string Id = $"{SaveLayoutToFileCommand.Id}.all";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.SaveAllLayoutToFileCommand_CommandInfo_Name,
        Description = RS.SaveAllLayoutToFileCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ContentSaveAll,
        DefaultHotKey = "Ctrl+Alt+L",
    };

    #endregion

    private readonly ILayoutService _layoutService;

    public SaveAllLayoutToFileCommand(ILayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IViewModel context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        await context.RequestSaveAllLayoutToFile(_layoutService, cancel);
        return null;
    }
}
