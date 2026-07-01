using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.IO;

public static class SettingsPageRegistrations
{
    extension(PagesRegistrations.Builder builder)
    {
        public Builder Settings => new(builder);

        public PagesRegistrations.Builder RegisterSettings(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure.Invoke(new Builder(builder));
            return builder;
        }
    }

    public class Builder(PagesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            this.RegisterConnectionSettingsSubPage();
            return this;
        }
    }
}
