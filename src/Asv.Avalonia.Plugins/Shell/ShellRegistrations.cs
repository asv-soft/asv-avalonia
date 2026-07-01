using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public static class ShellRegistrations
{
    extension(PluginsRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public PluginsRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(PluginsRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterPages();
            return this;
        }
    }
}
