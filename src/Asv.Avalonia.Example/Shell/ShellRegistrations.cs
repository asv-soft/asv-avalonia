using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class ShellRegistrations
{
    extension(ExampleRegistrations.Builder builder)
    {
        public Builder Shell => new(builder);

        public ExampleRegistrations.Builder RegisterShell(Action<Builder>? configure = null)
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
            this.RegisterHome();
            this.RegisterPages();
            return this;
        }
    }
}
