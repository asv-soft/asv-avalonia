using Asv.IO;

namespace Asv.Avalonia;

public interface IMavlinkConnectionService : IExportable
{
    public IProtocolRouter Router { get; set; }
    public IDeviceExplorer DevicesExplorer { get; set; }

    //public IExportInfo Source { get; }
    public void DisablePort(IProtocolPort port);
    public void EnablePort(IProtocolPort port);
    public void RemovePort(IProtocolPort port);
    public ValueTask EditPort(IProtocolPort port);
    
}