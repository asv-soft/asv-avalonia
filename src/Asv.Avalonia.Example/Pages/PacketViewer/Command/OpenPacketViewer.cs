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
        Name = "Open Packet Viewer",
        Description = "Command that opens packet viewer",
        Icon = PacketViewerViewModel.PageIcon,
        HotKeyInfo = new HotKeyInfo { DefaultHotKey = null },
        Source = SystemModule.Instance,
    };

    #endregion
    public override ICommandInfo Info => StaticInfo;
}
