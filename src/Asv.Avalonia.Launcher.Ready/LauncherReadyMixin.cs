using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Launcher.Ready;

public static class LauncherReadyMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseLauncher(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }

        public Builder ModuleLauncherReady => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder Parent => builder;

        public IHostApplicationBuilder RegisterDefault()
        {
            builder.Services.AddSingleton<LauncherNotifier>();
            builder.Services.AddHostedService<LauncherFeature>();
            return builder;
        }
    }
}
