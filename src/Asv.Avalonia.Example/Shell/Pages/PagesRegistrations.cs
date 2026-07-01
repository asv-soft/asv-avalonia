using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Example;

public static class PagesRegistrations
{
    extension(ShellRegistrations.Builder builder)
    {
        public Builder Pages => new(builder);

        public ShellRegistrations.Builder RegisterPages(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ShellRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
#if DEBUG
            this.RegisterMapTest();
#endif
            this.RegisterTextFile();
            this.RegisterControlsGallery();
            return this;
        }
    }
}
