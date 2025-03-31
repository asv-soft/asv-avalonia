using Asv.Avalonia.Example.PacketViewer.Command;
using Asv.Common;
using R3;

namespace Asv.Avalonia.Example.PacketViewer;

[ExportExtensionFor<IHomePage>]
public class HomePacketViewerExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenPacketViewerCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
