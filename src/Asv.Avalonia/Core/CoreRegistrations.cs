using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class CoreRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Core => new(builder);

        public IHostApplicationBuilder RegisterCore(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder RegisterDefault()
        {
            this.RegisterServices();
            this.RegisterControls();
            return this;
        }
    }
}
