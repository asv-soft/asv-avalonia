using Asv.Avalonia;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Charts;

public static class ControlsRegistrations
{
    extension(CoreRegistrations.Builder builder)
    {
        public Builder Controls => new(builder);

        public CoreRegistrations.Builder RegisterControls(Action<Builder>? configure = null)
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
            this.RegisterPlot();
            this.RegisterSignal();
            this.RegisterWorkspace();
            return this;
        }
    }
}
