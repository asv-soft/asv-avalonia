using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Charts;

public static class CoreRegistrations
{
    extension(ChartsRegistrations.Builder builder)
    {
        public Builder Core => new(builder);

        public ChartsRegistrations.Builder RegisterCore(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ChartsRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterControls();
            return this;
        }
    }
}
