using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.GeoMap;

public static class GeoMapMixin
{
    public const string MetricName = "asv.avalonia.map";

    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleGeoMap(Action<Builder>? configure = null)
        {
            configure ??= b =>
            {
                b.RegisterDefault();
            };
            configure(new Builder(builder));
            return builder;
        }

        public Builder ModuleGeoMap => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public void RegisterDefault()
        {
            builder.ModuleGeoMap.AddControls();
            builder.ModuleGeoMap.AddDialogs();
            builder.ModuleGeoMap.AddCommands();
            builder.ModuleGeoMap.AddServices();

            builder.ModuleGeoMap.AddGeoMapSettingsSubPage();
        }

        public IHostApplicationBuilder Parent => builder;

        public Builder RegisterTileProvider<TTileProvider>()
            where TTileProvider : class, ITileProvider
        {
            builder.Services.AddSingleton<ITileProvider, TTileProvider>();
            return this;
        }
    }
}
