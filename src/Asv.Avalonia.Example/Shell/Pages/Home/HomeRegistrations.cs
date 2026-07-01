using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class HomeRegistrations
{
    extension(ShellRegistrations.Builder builder)
    {
        public ShellRegistrations.Builder RegisterHome()
        {
            builder.AppBuilder.Extensions.Register<IHomePageItem, DeviceActionExample>();
            return builder;
        }
    }
}
