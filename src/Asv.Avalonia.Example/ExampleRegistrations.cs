using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class ExampleRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ExampleApp => new(builder);

        public IHostApplicationBuilder RegisterExampleApp(Action<Builder>? configure = null)
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
            this.RegisterCore();
            this.RegisterShell();
            return this;
        }
    }
}
