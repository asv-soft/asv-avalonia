namespace Asv.Avalonia;

public class OpenLogViewerCommand(INavigationService nav)
    : OpenPageCommandBase(LogViewerViewModel.PageId, nav)
{
    #region Static

    public const string Id = $"{BaseId}.open.{LogViewerViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.LogViewer_CommandInfo_Name,
        Description = RS.LogViewer_CommandInfo_Description,
        Icon = LogViewerViewModel.PageIcon,
        IconColor = LogViewerViewModel.PageIconColor,

        DefaultHotKey = "Ctrl+Shift+L",
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
