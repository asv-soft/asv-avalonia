using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Charts;

public static class ChartsRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ModuleCharts => new(builder);

        public IHostApplicationBuilder RegisterModuleCharts(Action<Builder>? configure = null)
        {
            configure ??= b =>
            {
                b.RegisterDefault();
            };
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public Builder RegisterDefault()
        {
            return this.RegisterCore();
        }

        public IHostApplicationBuilder AppBuilder => builder;
    }
}
