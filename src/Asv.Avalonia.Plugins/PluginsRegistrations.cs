using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Plugins;

public static class PluginsRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ModulePlugins => new(builder);

        public IHostApplicationBuilder RegisterModulePlugins(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterShell();
            this.RegisterCore();
            return this;
        }
    }
}
