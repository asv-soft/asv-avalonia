using Asv.Avalonia;
using Asv.Avalonia.IO;

namespace Asv.Avalonia.Example;

public static class DeviceManagerRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterDeviceManager()
        {
            builder.AppBuilder.DeviceManager.RegisterDevice<ExampleDeviceManagerExtension>();
            return builder;
        }
    }
}
