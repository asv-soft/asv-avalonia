using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public interface IDeviceManager
{
    IProtocolFactory ProtocolFactory { get; }
    MaterialIconKind? GetIcon(DeviceId id);
    AsvColorKind GetDeviceColor(DeviceId id);
    IProtocolRouter Router { get; }
    IDeviceExplorer Explorer { get; }
}
