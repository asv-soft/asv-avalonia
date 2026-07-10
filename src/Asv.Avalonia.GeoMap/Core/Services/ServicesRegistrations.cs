using Asv.Avalonia;
using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia.GeoMap;

public static class ServicesRegistrations
{
    public static CoreRegistrations.Builder RegisterServices(this CoreRegistrations.Builder builder)
    {
        builder
            .AppBuilder.Services.AddKeyedSingleton<ITileCache, MemoryTileCache>(
                TileLoader.FastTileCacheContract
            )
            .AddOptions<MemoryTileCacheConfig>()
            .BindConfiguration(MemoryTileCacheConfig.ConfigurationSection);

        builder
            .AppBuilder.Services.AddKeyedSingleton<ITileCache, FileSystemCache>(
                TileLoader.SlowTileCacheContract
            )
            .AddOptions<FileSystemCacheConfig>()
            .BindConfiguration(FileSystemCacheConfig.ConfigurationSection);

        builder.AppBuilder.Services.AddHttpClient(
            HttpTileProvider.HttpClientName,
            (sp, client) =>
            {
                var config = sp.GetRequiredService<IConfiguration>().Get<HttpTileProviderConfig>();
                var appInfo = sp.GetRequiredService<IAppInfo>();
                client.Timeout = TimeSpan.FromMilliseconds(config.RequestTimeoutMs);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(
                    $"{typeof(GeoMapRegistrations).Namespace}/{appInfo.Version}"
                );
            }
        );

        builder.AppBuilder.Services.AddSingleton<IMapService, MapService>();
        builder.AppBuilder.Services.AddSingleton<ITileLoader, TileLoader>();
        builder.AppBuilder.Services.AddSingleton<ITileProviderService, TileProviderService>();
        builder.AppBuilder.Services.AddSingleton<IZoomService, ZoomService>();

        builder
            .AppBuilder.ViewModel.RegisterTileProvider<ThunderforestLandscapeTileProvider>()
            .RegisterTileProvider<ThunderforestCycleTileProvider>()
            .RegisterTileProvider<ThunderforestTransportTileProvider>()
            .RegisterTileProvider<ThunderforestOutdoorsTileProvider>()
            .RegisterTileProvider<ThunderforestTransportDarkTileProvider>()
            .RegisterTileProvider<ThunderforestAtlasTileProvider>()
            .RegisterTileProvider<HereMapTileProvider>()
            .RegisterTileProvider<HereSatelliteTileProvider>()
            .RegisterTileProvider<HereHybridTileProvider>()
            .RegisterTileProvider<HereTerrainTileProvider>();

        builder
            .AppBuilder.ViewModel.RegisterTileProvider<YandexMapTileProvider>()
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
