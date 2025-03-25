using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public interface IDeviceManager
{
    MaterialIconKind? GetIcon(DeviceId id);
    IBrush? GetDeviceBrush(DeviceId id);
    IProtocolRouter Router { get; }
    IDeviceExplorer Explorer { get; }
    MaterialIconKind? GetIcon(PortTypeInfo portTypeInfo);
}
