using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.GeoMap;

public static class ShellRegistrations
{
    extension(GeoMapRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public GeoMapRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(GeoMapRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterPages();
            this.RegisterMapStatus();
            return this;
        }
    }
}
