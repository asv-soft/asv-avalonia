using Asv.IO;

namespace Asv.Avalonia.IO;

public interface IDevicePage : IPage
{
    IClientDevice? Device { get; }

    Action<IClientDevice>? AfterDeviceInitializedCallback { set; }
}
