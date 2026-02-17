using Material.Icons;

namespace Asv.Avalonia;

public sealed class SaveLayoutToFileCommand : ContextCommand<IRoutable>
{
    #region Static

    public const string Id = $"{BaseId}.layout.save";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.SaveLayoutToFileCommand_CommandInfo_Name,
        Description = RS.SaveLayoutToFileCommand_CommandInfo_Description,
        Icon = MaterialIconKind.ContentSave,
        DefaultHotKey = "Ctrl+L",
    };

    #endregion

    private readonly ILayoutService _layoutService;

    public SaveLayoutToFileCommand(ILayoutService layoutService)
    {
        _layoutService = layoutService;
    }

    public override ICommandInfo Info => StaticInfo;

    protected override async ValueTask<CommandArg?> InternalExecute(
        IRoutable context,
        CommandArg newValue,
        CancellationToken cancel
    )
    {
        var current = context;
        while (current is not null)
        {
            if (current is IPage page)
            {
                await page.RequestSaveLayoutToFile(_layoutService, cancel);
                return null;
            }

            if (current.Parent is null)
            {
                if (current is not ShellViewModel model)
                {
                    return null;
                }

                if (model.SelectedPage.Value is null)
                {
                    return null;
                }

                await model.SelectedPage.Value.RequestSaveLayoutToFile(_layoutService, cancel);
                return null;
            }

            current = current.Parent;
        }

        return null;
    }
}
