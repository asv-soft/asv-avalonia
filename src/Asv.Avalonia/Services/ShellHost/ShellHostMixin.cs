using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ShellHostMixin
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder UseShellHost()
        {
            builder.Services.AddSingleton<IShellHost, ShellHost>();
            return builder;
        }

        internal IHostApplicationBuilder UseShellHostForDesignTime()
        {
            builder.Services.AddSingleton<IShellHost>(NullShellHost.Instance);
            return builder;
        }
    }
}
