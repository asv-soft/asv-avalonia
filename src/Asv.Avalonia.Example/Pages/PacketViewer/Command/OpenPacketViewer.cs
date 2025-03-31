using System.Composition;

namespace Asv.Avalonia.Example.PacketViewer.Command;

[ExportCommand]
[method: ImportingConstructor]
public class OpenPacketViewerCommand(INavigationService nav)
    : OpenPageCommandBase(PacketViewerViewModel.PageId, nav)
{
    #region Static
    public const string Id = $"{BaseId}.open.{PacketViewerViewModel.PageId}";

    public static readonly ICommandInfo StaticInfo = new CommandInfo
    {
        Id = Id,
        Name = RS.PacketViewerViewDockPanelText,
        Description = RS.PacketViewer_Open,
        Icon = PacketViewerViewModel.PageIcon,
        DefaultHotKey = null,
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
