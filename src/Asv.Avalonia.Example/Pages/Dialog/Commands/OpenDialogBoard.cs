using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenDialogBoardCommand(INavigationService nav)
    : OpenPageCommandBase(DialogBoardViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.dialog_board";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = "Open dialog board",
        Description = "Commands that opens the dialog board",
        Icon = DialogBoardViewModel.PageIcon,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
