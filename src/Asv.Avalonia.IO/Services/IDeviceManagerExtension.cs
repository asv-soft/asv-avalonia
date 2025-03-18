using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public interface IDeviceManagerExtension
{
    void Configure(IProtocolBuilder builder);
    void Configure(IDeviceExplorerBuilder builder);
    bool TryGetIcon(DeviceId id, out MaterialIconKind? icon);
    bool TryGetDeviceBrush(DeviceId id, out IBrush? brush);
}
