using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.GeoMap;

public static class GeoMapMixin
{
    public static IHostApplicationBuilder UseAsvGeoMap(this IHostApplicationBuilder builder)
    {
        var options = builder
            .Services.AddOptions<GeoMapOptions>()
            .Bind(builder.Configuration.GetSection(GeoMapOptions.Section));

        builder.Services.AddSingleton<ITileLoader, TileLoader>();

        builder
            .RegisterViewFor<GeoPointDialogViewModel, GeoPointDialogView>()
            .RegisterViewFor<MapWidget, MapWidgetView>();
        var subBuilder = new GeoMapBuilder();
        subBuilder.Build(options);
        return builder;
    }
}
