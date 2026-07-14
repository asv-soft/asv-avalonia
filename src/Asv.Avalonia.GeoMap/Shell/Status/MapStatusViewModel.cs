using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapStatusViewModel : StatusItem
{
    public const string TypeId = "status_map";

    private readonly IncrementalRateCounter _downloadBytes;
    private readonly IncrementalRateCounter _downloadTiles;
    private readonly IDataFormatter _dataSizeFormatter;
    private readonly IDataFormatter _byteRateFormatter;

    public MapStatusViewModel()
        : this(DesignTime.UnitService, TimeProvider.System)
    {
        DesignTime.ThrowIfNotDesignMode();

        var downloadedBytes = 0L;
        var downloadedTiles = 0L;
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                downloadedBytes += Random.Shared.Next(0, 2_000_000);
                downloadedTiles += Random.Shared.Next(0, 20);

                var loader = new TileLoaderStatistic(
                    Random.Shared.Next(1_000, 500_000),
                    Random.Shared.Next(0, 100),
                    Random.Shared.Next(0, 8),
                    100,
                    Environment.ProcessorCount,
                    Random.Shared.Next(1_000, 50_000),
                    Random.Shared.Next(1_000, 25_000),
                    downloadedTiles,
                    downloadedBytes,
                    Random.Shared.Next(0, 100),
                    MapModeType.Mixed
                );
                var memory = new TileCacheStatistic(12_340, 120, 980, 42_000_000, 100_000_000);
                var file = new TileCacheStatistic(
                    85_000,
                    5_000,
                    42_000,
                    780_000_000,
                    2_000_000_000
                );

                UpdateStatistic(loader, memory, file);
            })
            .DisposeItWith(Disposable);
    }

    public MapStatusViewModel(
        ITileLoader tileLoader,
        [FromKeyedServices(TileLoader.FastTileCacheContract)] ITileCache fastCache,
        [FromKeyedServices(TileLoader.SlowTileCacheContract)] ITileCache slowCache,
        IUnitService unitService,
        TimeProvider timeProvider
    )
        : this(unitService, timeProvider)
    {
        Observable
            .Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
                UpdateStatistic(
                    tileLoader.GetStatistic(),
                    fastCache.GetStatistic(),
                    slowCache.GetStatistic()
                )
            )
            .DisposeItWith(Disposable);
        this.ObservePropertyChanged(x => x.IsFlyoutOpen)
            .Subscribe(_ =>
                UpdateStatistic(
                    tileLoader.GetStatistic(),
                    fastCache.GetStatistic(),
                    slowCache.GetStatistic()
                )
            )
            .DisposeItWith(Disposable);
    }

    private MapStatusViewModel(IUnitService unitService, TimeProvider timeProvider)
        : base(TypeId, default)
    {
        _dataSizeFormatter = unitService.CreateDataSizeFormatter();
        _byteRateFormatter = unitService.CreateByteRateFormatter();
        _downloadBytes = new IncrementalRateCounter(5, timeProvider);
        _downloadTiles = new IncrementalRateCounter(5, timeProvider);
        UpdateStatistic(
            TileLoaderStatistic.Empty,
            TileCacheStatistic.Empty,
            TileCacheStatistic.Empty
        );
    }

    public override int Order => 240;

    public bool IsFlyoutOpen
    {
        get;
        set => SetField(ref field, value);
    }

    public string NetworkRateText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string QueueSizeText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MapModeText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string QueueDetailsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ActiveRequestsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string ParallelDownloadsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string TotalRequestsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string QueuedRequestsTotalText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string NetworkRequestsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string NetworkDownloadedTilesText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string NetworkDownloadedSizeText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string FailedDownloadsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MemoryCacheSizeText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MemoryCacheTilesText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MemoryCacheHitsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MemoryCacheMissesText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MapFolderSizeText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string MapFolderCapacityText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string DownloadedTilesText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string FileCacheHitsText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public string FileCacheMissesText
    {
        get;
        set => SetField(ref field, value);
    } = string.Empty;

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    public void NavigateToSettings()
    {
        this.GoTo(
                new NavPath(
                    new NavId(SettingsPageViewModel.PageId),
                    new NavId(SettingsGeoMapViewModel.PageId)
                )
            )
            .SafeFireAndForget();
    }

    private void UpdateStatistic(
        TileLoaderStatistic loader,
        TileCacheStatistic fastCache,
        TileCacheStatistic slowCache
    )
    {
        var bytesRate = _downloadBytes.Calculate(loader.DownloadedBytes);
        var tilesRate = _downloadTiles.Calculate(loader.DownloadedTiles);

        NetworkRateText = _byteRateFormatter.Print(bytesRate);
        QueueSizeText = $"{loader.QueuedRequests:N0}/{loader.RequestQueueCapacity:N0}";

        MapModeText = FormatMapMode(loader.MapMode);
        QueueDetailsText = $"{loader.QueuedRequests:N0} / {loader.RequestQueueCapacity:N0}";
        ActiveRequestsText = loader.ActiveRequests.ToString("N0");
        ParallelDownloadsText = loader.RequestParallelThreads.ToString("N0");
        TotalRequestsText = loader.Requests.ToString("N0");
        QueuedRequestsTotalText = loader.QueuedRequestsTotal.ToString("N0");
        NetworkRequestsText = loader.NetworkRequests.ToString("N0");
        NetworkDownloadedTilesText = $"{loader.DownloadedTiles:N0} ({tilesRate:F1}/s)";
        NetworkDownloadedSizeText = _dataSizeFormatter.Print(loader.DownloadedBytes);
        FailedDownloadsText = loader.FailedDownloads.ToString("N0");

        MemoryCacheSizeText = FormatSizeWithCapacity(fastCache.Size, fastCache.CapacitySize);
        MemoryCacheTilesText = fastCache.TileCount.ToString("N0");
        MemoryCacheHitsText = fastCache.Hits.ToString("N0");
        MemoryCacheMissesText = fastCache.Misses.ToString("N0");

        MapFolderSizeText = _dataSizeFormatter.Print(slowCache.Size);
        MapFolderCapacityText = _dataSizeFormatter.Print(slowCache.CapacitySize);
        DownloadedTilesText = slowCache.TileCount.ToString("N0");
        FileCacheHitsText = slowCache.Hits.ToString("N0");
        FileCacheMissesText = slowCache.Misses.ToString("N0");
    }

    private string FormatSizeWithCapacity(long size, long capacity)
    {
        return $"{_dataSizeFormatter.Print(size)} / {_dataSizeFormatter.Print(capacity)}";
    }

    private static string FormatMapMode(MapModeType mode)
    {
        return mode switch
        {
            MapModeType.Mixed => RS.MapModeProperty_MapModeInfo_Mixed,
            MapModeType.Online => RS.MapModeProperty_MapModeInfo_Online,
            MapModeType.Offline => RS.MapModeProperty_MapModeInfo_Offline,
            _ => mode.ToString(),
        };
    }
}
