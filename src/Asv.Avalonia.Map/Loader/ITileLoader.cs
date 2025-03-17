using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using R3;

namespace Asv.Avalonia.Map;

public static class MapMixin
{
    public static IHostApplicationBuilder UseAsvMap(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<ITileLoader, TileLoader>();
        return builder;
    }
}

public interface ITileLoader
{
    Bitmap this[TileKey key] { get; }
    Observable<TileKey> OnLoaded { get; }
    ReactiveProperty<IBrush> EmptyTileBrush { get; }
}
