using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class CoreRegistrations
{
    extension(IoRegistrations.Builder builder)
    {
        public Builder Core => new(builder);

        public IoRegistrations.Builder RegisterCore(Action<Builder>? configure = null)
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
            this.RegisterServices();
            return this;
        }
    }
}
