using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public class NullDeviceManager : IDeviceManager
{
    public static IDeviceManager Instance { get; } = new NullDeviceManager();

    private NullDeviceManager()
    {
        ProtocolFactory = Protocol.Create(builder =>
        {
            builder.Protocols.RegisterExampleProtocol();
        });

        Router = ProtocolFactory.CreateRouter("DesignTime");
        Explorer = DeviceExplorer.Create(Router, builder => { });
    }

    public IProtocolFactory ProtocolFactory { get; }

    public MaterialIconKind? GetIcon(DeviceId id)
    {
        return MaterialIconKind.Navigation;
    }

    public IBrush? GetDeviceBrush(DeviceId id)
    {
        return Brushes.Aqua;
    }

    public IProtocolRouter Router { get; }
    public IDeviceExplorer Explorer { get; }
}
