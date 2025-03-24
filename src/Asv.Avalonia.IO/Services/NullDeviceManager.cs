using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public class NullDeviceManager : IDeviceManager
{
    public static IDeviceManager Instance { get; } = new NullDeviceManager();
    
    private NullDeviceManager()
    {
        var factory = Protocol.Create(builder =>
        {
            builder.Protocols.RegisterExampleProtocol();
        });

        Router = factory.CreateRouter("DesignTime");
        Explorer = DeviceExplorer.Create(Router, builder =>
        {
            
        });
    }
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