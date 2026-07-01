using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

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
            this.RegisterAppArgsStore();
            this.RegisterAppInfo();
            this.RegisterAppPath();
            this.RegisterRestartFeature();
            this.RegisterDialogs();
            this.RegisterExtensions();
            this.RegisterFileAssociation();
            this.RegisterUnhandledExceptionsHandler();
            this.RegisterHotKeys();
            this.RegisterLocalizationService();
            this.RegisterLogViewer();
            this.RegisterLogToFile();
            this.RegisterSearchService();
            this.RegisterShellHost();
            this.RegisterSoloRun();
            this.RegisterThemeService();
            this.RegisterTimeProvider();
            this.RegisterUnitService();
            this.RegisterUserConfig();
            this.RegisterViewLocator();
            return this;
        }
    }
}
