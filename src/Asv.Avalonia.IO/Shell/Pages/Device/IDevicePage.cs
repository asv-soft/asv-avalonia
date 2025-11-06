using R3;

namespace Asv.Avalonia.IO;

public interface IDevicePage : IPage
{
    ReadOnlyReactiveProperty<DeviceWrapper?> Target { get; }
}
