using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.GeoMap;

public static class GeoMapRegistrations
{
    public const string MetricName = "asv.avalonia.map";

    extension(IHostApplicationBuilder builder)
    {
        public Builder ModuleGeoMap => new(builder);

        public IHostApplicationBuilder RegisterModuleGeoMap(Action<Builder>? configure = null)
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
