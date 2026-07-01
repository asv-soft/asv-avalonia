using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellHostRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterShellHost()
        {
            builder.AppBuilder.Services.AddSingleton<IShellHost, ShellHost>();
            return builder;
        }
    }
}
