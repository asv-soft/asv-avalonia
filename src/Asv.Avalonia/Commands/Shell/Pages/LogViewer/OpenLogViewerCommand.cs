using System.Composition;

namespace Asv.Avalonia;

[ExportCommand]
[method: ImportingConstructor]
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
        Source = SystemModule.Instance,
        DefaultHotKey = "Ctrl+Shift+L",
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
