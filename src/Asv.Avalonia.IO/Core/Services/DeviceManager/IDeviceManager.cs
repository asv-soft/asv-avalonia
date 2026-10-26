using System.Diagnostics.CodeAnalysis;
using Asv.IO;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia.IO;

public interface IDeviceManager
{
    bool TryIdentify(IProtocolMessage message, [NotNullWhen(true)] out DeviceId? deviceId);
    bool TryGetSourceAddress(IProtocolMessage message, out DeviceAddress address) =>
        Explorer.TryGetSourceAddress(message, out address);
    IProtocolFactory ProtocolFactory { get; }
    MaterialIconKind? GetIcon(DeviceId id);
    AsvColorKind GetDeviceColor(DeviceId id);
    IProtocolRouter Router { get; }
    IDeviceExplorer Explorer { get; }
}
