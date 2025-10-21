using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportCommand]
[Shared]
public class SaveLayoutCommand : ContextCommand<IRoutable>
{
    #region Static

    public const string Id = $"{BaseId}.save";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Save Layout",
        Description = "Command that saves layout of the current page to file",
        Icon = MaterialIconKind.ContentSave,
        DefaultHotKey = "Ctrl+L",
        Source = SystemModule.Instance,
    };

    #endregion

    private readonly ILayoutService _layoutService;

    [ImportingConstructor]
    public SaveLayoutCommand(ILayoutService layoutService)
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
