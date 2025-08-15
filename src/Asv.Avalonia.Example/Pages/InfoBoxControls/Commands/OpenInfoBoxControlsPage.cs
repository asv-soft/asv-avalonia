using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenTestInfoBoxPageCommand(INavigationService nav)
    : OpenPageCommandBase(InfoBoxControlsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.info_box_controls";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenInfoBoxCommand_Action_Title,
        Description = RS.OpenInfoBoxCommand_Action_Description,
        Icon = InfoBoxControlsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
