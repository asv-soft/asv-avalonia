using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class ServicesRegistrations
{
    extension(CoreRegistrations.Builder builder)
    {
        public Builder Services => new(builder);

        public CoreRegistrations.Builder RegisterServices(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(CoreRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterDeviceManager();
            return this;
        }
    }
}
