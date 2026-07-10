using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class IoRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ModuleIo => new(builder);

        public IHostApplicationBuilder RegisterModuleIo(Action<Builder>? configure = null)
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
