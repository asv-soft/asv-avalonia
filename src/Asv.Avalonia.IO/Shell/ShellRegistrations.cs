using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class ShellRegistrations
{
    extension(IoRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public IoRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IoRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterPages();
            this.RegisterConnectionStatus();
            return this;
        }
    }
}
