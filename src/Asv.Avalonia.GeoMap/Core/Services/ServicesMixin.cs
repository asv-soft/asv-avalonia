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

        builder.Parent.Services.AddSingleton<IMapService, MapService>();
        builder.Parent.Services.AddSingleton<ITileLoader, TileLoader>();
        builder.Parent.Services.AddSingleton<ITileProviderService, TileProviderService>();
        builder.Parent.Services.AddSingleton<IZoomService, ZoomService>();

        builder
            .RegisterTileProvider<YandexTileProvider>()
            .RegisterTileProvider<BingTileProvider>()
            .RegisterTileProvider<EmptyTileProvider>();

        builder
            .Parent.Services.AddOptions<BingTileProviderOptions>()
            .BindConfiguration(BingTileProviderOptions.ConfigurationSection);

        return builder;
    }
}
