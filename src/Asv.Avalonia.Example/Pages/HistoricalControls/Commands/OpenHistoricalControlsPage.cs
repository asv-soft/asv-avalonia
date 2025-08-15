using System.Composition;

namespace Asv.Avalonia.Example;

[ExportCommand]
[method: ImportingConstructor]
public class OpenTestHistoryPropertiesPageCommand(INavigationService nav)
    : OpenPageCommandBase(HistoricalControlsPageViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.historical_controls";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.OpenHistoricalCommand_Action_Title,
        Description = RS.OpenHistoricalCommand_Action_Description,
        Icon = HistoricalControlsPageViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
