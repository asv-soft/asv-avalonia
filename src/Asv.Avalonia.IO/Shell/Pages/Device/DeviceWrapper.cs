using Asv.IO;

namespace Asv.Avalonia.IO;

public readonly struct DeviceWrapper(IClientDevice device, CancellationToken whenDisconnectedToken)
{
    public IClientDevice Device { get; } = device;
    public CancellationToken WhenDisconnectedToken { get; } = whenDisconnectedToken;
}
