namespace Asv.Avalonia.GeoMap;

#pragma warning disable SA1313
public sealed record MapPreloadProgress(
    long TotalTiles,
    long ProcessedTiles,
    long CachedTiles,
    long DownloadedTiles,
    long FailedTiles,
    long DownloadedBytes,
    string? CurrentProvider = null
);
#pragma warning restore SA1313

public interface IMapPreloadService
{
    long CountTiles(MapPreloadRequest request, CancellationToken cancellationToken = default);
    Task<MapPreloadProgress> DownloadAsync(
        MapPreloadRequest request,
        IProgress<MapPreloadProgress>? progress = null,
        CancellationToken cancellationToken = default
    );
}
