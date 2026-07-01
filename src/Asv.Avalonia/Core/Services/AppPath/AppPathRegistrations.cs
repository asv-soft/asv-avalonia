using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppPathRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterAppPath()
        {
            builder.AppBuilder.Services.AddSingleton<IAppPath, AppPath>();
            return builder;
        }
    }
}
