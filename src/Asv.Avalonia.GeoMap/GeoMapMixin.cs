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
            builder
                .Services.AddKeyedSingleton<ITileCache, MemoryTileCache>(
                    TileLoader.FastTileCacheContract
                )
                .AddOptions<MemoryTileCacheConfig>()
                .BindConfiguration(MemoryTileCacheConfig.ConfigurationSection);

            builder
                .Services.AddKeyedSingleton<ITileCache, FileSystemCache>(
                    TileLoader.SlowTileCacheContract
                )
                .AddOptions<FileSystemCacheConfig>()
                .BindConfiguration(FileSystemCacheConfig.ConfigurationSection);

            builder.Services.AddSingleton<ITileLoader, TileLoader>();

            builder.Dialogs.RegisterPrefab<GeoPointDialogPrefab>();
            builder
                .ViewLocator.RegisterViewFor<GeoPointDialogViewModel, GeoPointDialogView>()
                .RegisterViewFor<MapViewModel, MapView>()
                .RegisterViewFor<MapWidget, MapWidgetView>();
            this.RegisterTileProvider<YandexTileProvider>()
                .RegisterTileProvider<BingTileProvider>()
                .RegisterTileProvider<EmptyTileProvider>();

            builder
                .Services.AddOptions<BingTileProviderOptions>()
                .BindConfiguration(BingTileProviderOptions.ConfigurationSection);
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
