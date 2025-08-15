using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenMapControlsPageCommand(INavigationService nav)
    : OpenPageCommandBase(MapControlsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.map";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenMapExampleCommand_Action_Title,
        Description = RS.OpenMapExampleCommand_Action_Description,
        Icon = MapControlsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
