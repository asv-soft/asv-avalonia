using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.GeoMap;

public static class CoreRegistrations
{
    extension(GeoMapRegistrations.Builder builder)
    {
        public Builder Core => new(builder);

        public GeoMapRegistrations.Builder RegisterCore(Action<Builder>? configure = null)
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
            this.RegisterServices();
            this.RegisterControls();
            this.RegisterDialogs();

            return this;
        }
    }
}
