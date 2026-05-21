using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppPathMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseAppPath()
        {
            builder.Services.AddSingleton<IAppPath, AppPath>();
            return builder;
        }
    }
}
