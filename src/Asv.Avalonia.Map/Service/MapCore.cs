using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.Map;

public static class MapMixin
{
    public static AppHostBuilder UseAsvMap(this AppHostBuilder builder)
    {
        builder.Services.AddSingleton<IMapService>();
        return builder;
    }
}

public interface IMapService
{
    IEnumerable<ITileProvider> Providers { get; }
    Bitmap this[TileKey key] { get; }
    Observable<TileLoadedEventArgs> OnLoaded { get; }
    ReactiveProperty<IBrush> EmptyTileBrush { get; }
}
