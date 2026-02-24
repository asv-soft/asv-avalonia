using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppHostMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseSystemTimeProvider()
        {
            builder.Services.AddSingleton(TimeProvider.System);
            return builder;
        }

        public IHostApplicationBuilder UseShellHost()
        {
            builder.Services.AddSingleton<IShellHost, ShellHost>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeShellHost()
        {
            builder.Services.AddSingleton<IShellHost>(NullShellHost.Instance);
            return builder;
        }
    }
}
