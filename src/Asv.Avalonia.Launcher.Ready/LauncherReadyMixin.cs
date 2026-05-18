using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Launcher.Ready;

public static class LauncherReadyMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleLauncherReady(Action<Builder>? configure = null)
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
            builder.Extensions.Register<IShell, LauncherReadyShellExtension>();
            return builder;
        }
    }
}
