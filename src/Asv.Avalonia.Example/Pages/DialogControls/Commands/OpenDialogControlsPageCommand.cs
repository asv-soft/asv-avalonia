using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenDialogControlsPageCommand(INavigationService nav)
    : OpenPageCommandBase(DialogControlsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.dialog_controls";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenDialogBoardCommand_Action_Title,
        Description = RS.OpenDialogBoardCommand_Action_Description,
        Icon = DialogControlsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
