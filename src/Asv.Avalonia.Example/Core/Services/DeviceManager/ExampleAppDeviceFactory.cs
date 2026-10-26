using System.Globalization;
using Asv.IO;
using Asv.IO.Device;

namespace Asv.Avalonia.Example;

public sealed class ExampleAppDeviceFactory(ExampleDeviceConfig config) : ExampleDeviceFactory(config)
{
    public override bool TryGetSourceAddress(IProtocolMessage message, out DeviceAddress address)
    {
        address = default;
        if (message is not ExampleMessageBase example)
        {
            return false;
        }
        address = CreateAddress(example.SenderId);
        return true;
    }

    public override bool TryGetDeviceAddress(DeviceId deviceId, out DeviceAddress address)
    {
        address = default;
        if (deviceId is not ExampleDeviceId example)
        {
            return false;
        }
        address = CreateAddress(example.TargetId);
        return true;
    }

    private static DeviceAddress CreateAddress(byte id) => new("example", id.ToString(CultureInfo.InvariantCulture));
}
