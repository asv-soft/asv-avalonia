using Avalonia.Media;
using Avalonia.Media.Imaging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileLoaderStatistic(
    long requests,
    int queuedRequests,
    int activeRequests,
    int requestQueueCapacity,
    int requestParallelThreads,
    long queuedRequestsTotal,
    long networkRequests,
    long downloadedTiles,
    long downloadedBytes,
    long failedDownloads,
    MapModeType mapMode
)
{
    public static TileLoaderStatistic Empty { get; } =
        new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, MapModeType.Mixed);

    public long Requests { get; } = requests;
    public int QueuedRequests { get; } = queuedRequests;
    public int ActiveRequests { get; } = activeRequests;
    public int RequestQueueCapacity { get; } = requestQueueCapacity;
    public int RequestParallelThreads { get; } = requestParallelThreads;
    public long QueuedRequestsTotal { get; } = queuedRequestsTotal;
    public long NetworkRequests { get; } = networkRequests;
    public long DownloadedTiles { get; } = downloadedTiles;
    public long DownloadedBytes { get; } = downloadedBytes;
    public long FailedDownloads { get; } = failedDownloads;
    public MapModeType MapMode { get; } = mapMode;
}

public interface ITileLoader
{
    Observable<TileKey> OnLoaded { get; }
    SynchronizedReactiveProperty<IBrush> EmptyTileBrush { get; }
    SynchronizedReactiveProperty<MapModeType> CurrentMapMode { get; }
    TileLoaderStatistic GetStatistic();
    void Render(DrawingContext context, double x, double y, TileKey key);
}
