using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class ServicesMixin
{
    public static GeoMapMixin.Builder AddServices(this GeoMapMixin.Builder builder)
    {
        builder
            .Parent.Services.AddKeyedSingleton<ITileCache, MemoryTileCache>(
                TileLoader.FastTileCacheContract
            )
            .AddOptions<MemoryTileCacheConfig>()
            .BindConfiguration(MemoryTileCacheConfig.ConfigurationSection);

        builder
            .Parent.Services.AddKeyedSingleton<ITileCache, FileSystemCache>(
                TileLoader.SlowTileCacheContract
            )
            .AddOptions<FileSystemCacheConfig>()
            .BindConfiguration(FileSystemCacheConfig.ConfigurationSection);

        builder.Parent.Services.AddHttpClient(
            TileLoader.HttpClientName,
            (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>().Get<TileLoaderConfig>();
                var appInfo = sp.GetRequiredService<IAppInfo>();
                client.Timeout = TimeSpan.FromMilliseconds(config.RequestTimeoutMs);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    $"{typeof(GeoMapMixin).Namespace}/{appInfo.Version}"
                );
            }
        );

        builder.Parent.Services.AddSingleton<IMapService, MapService>();
        builder.Parent.Services.AddSingleton<ITileLoader, TileLoader>();
        builder.Parent.Services.AddSingleton<ITileProviderService, TileProviderService>();
        builder.Parent.Services.AddSingleton<IZoomService, ZoomService>();

        // TODO: handle auth errors gracefully
        /*
        builder
            .RegisterTileProvider<ThunderforestLandscapeTileProvider>()
            .RegisterTileProvider<ThunderforestCycleTileProvider>()
            .RegisterTileProvider<ThunderforestTransportTileProvider>()
            .RegisterTileProvider<ThunderforestOutdoorsTileProvider>()
            .RegisterTileProvider<ThunderforestTransportDarkTileProvider>()
            .RegisterTileProvider<ThunderforestAtlasTileProvider>();
            .RegisterTileProvider<HereMapTileProvider>()
            .RegisterTileProvider<HereSatelliteTileProvider>()
            .RegisterTileProvider<HereHybridTileProvider>()
            .RegisterTileProvider<HereTerrainTileProvider>() */

        builder
            .RegisterTileProvider<YandexMapTileProvider>()
            .RegisterTileProvider<YandexSatelliteTileProvider>()
            .RegisterTileProvider<BingHybridTileProvider>()
            .RegisterTileProvider<BingRoadTileProvider>()
            .RegisterTileProvider<BingSatelliteTileProvider>()
            .RegisterTileProvider<OpenStreetMapTileProvider>()
            .RegisterTileProvider<CyclOsmTileProvider>()
            .RegisterTileProvider<HotOsmTileProvider>()
            .RegisterTileProvider<UmpTileProvider>()
            .RegisterTileProvider<GoogleMapTileProvider>()
            .RegisterTileProvider<GoogleSatelliteTileProvider>()
            .RegisterTileProvider<GoogleHybridTileProvider>()
            .RegisterTileProvider<GoogleTerrainTileProvider>()
            .RegisterTileProvider<ArcGisWorldStreetTileProvider>()
            .RegisterTileProvider<ArcGisWorldTopoTileProvider>()
            .RegisterTileProvider<ArcGisWorldTerrainTileProvider>()
            .RegisterTileProvider<ArcGisWorldShadedReliefTileProvider>()
            .RegisterTileProvider<ArcGisWorldPhysicalTileProvider>()
            .RegisterTileProvider<ArcGisWorldImageryTileProvider>()
            .RegisterTileProvider<WikiMapiaTileProvider>()
            .RegisterTileProvider<EmptyTileProvider>();

        return builder;
    }
}
