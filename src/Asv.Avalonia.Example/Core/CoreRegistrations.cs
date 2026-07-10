using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class CoreRegistrations
{
    extension(ExampleRegistrations.Builder builder)
    {
        public Builder Core => new(builder);

        public ExampleRegistrations.Builder RegisterCore(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ExampleRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterServices();
            this.RegisterControls();
            return this;
        }
    }
}
