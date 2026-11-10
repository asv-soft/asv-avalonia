using System.Collections.Concurrent;
using System.Diagnostics;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class MapPreloadServiceTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public async Task Download_SkipsDiskCacheAndCountsOnlyDurablySavedTiles()
    {
        using var cache = new PreloadCacheFixture();
        var first = Provider("first");
        var second = Provider("second");
        var cachedKey = new TileKey(2, 1, 2, first);
        using (var tile = await CreateTile(cachedKey))
        {
            await cache.Cache.SaveAsync(tile, TestContext.Current.CancellationToken);
        }
        var request = Request(first, second) with { MinZoom = 2, MaxZoom = 3 };
        var service = new MapPreloadService(cache.Cache);
        var reports = new List<MapPreloadProgress>();

        var result = await service.DownloadAsync(
            request,
            new InlineProgress(reports.Add),
            TestContext.Current.CancellationToken
        );

        Assert.Equal(4, service.CountTiles(request, TestContext.Current.CancellationToken));
        Assert.Equal(4, result.TotalTiles);
        Assert.Equal(4, result.ProcessedTiles);
        Assert.Equal(1, result.CachedTiles);
        Assert.Equal(3, result.DownloadedTiles);
        Assert.Equal(0, result.FailedTiles);
        Assert.Equal(3 * PreloadTestProvider.TileBytes.Length, result.DownloadedBytes);
        Assert.Equal(3, first.Requests.Count + second.Requests.Count);
        Assert.Equal(
            4,
            Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories).Length
        );
        Assert.Equal(result, reports[^1]);
        Assert.All(
            reports,
            report =>
                Assert.Equal(
                    report.ProcessedTiles,
                    report.CachedTiles + report.DownloadedTiles + report.FailedTiles
                )
        );
    }

    [Fact]
    public async Task CancellationAfterDownload_PersistsReturnedTileAndResumeSkipsDiskBytes()
    {
        using var cache = new PreloadCacheFixture();
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        var provider = Provider("resume");
        provider.Download = async (key, _) =>
        {
            var tile = await CreateTile(key);
            cancel.Cancel();
            return tile;
        };
        var request = Request(provider) with { MinZoom = 3, MaxZoom = 3, Concurrency = 1 };
        var reports = new List<MapPreloadProgress>();
        var service = new MapPreloadService(cache.Cache);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.DownloadAsync(request, new InlineProgress(reports.Add), cancel.Token)
        );

        var path = Assert.Single(
            Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories)
        );
        Assert.Equal(
            PreloadTestProvider.TileBytes,
            await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken)
        );
        Assert.Equal(1, reports[^1].DownloadedTiles);
        Assert.Equal(PreloadTestProvider.TileBytes.Length, reports[^1].DownloadedBytes);
        await cache.Cache.DisposeAsync();
        using var reopened = cache.CreateCache();
        var resumed = await new MapPreloadService(reopened).DownloadAsync(
            request,
            cancellationToken: TestContext.Current.CancellationToken
        );
        Assert.Equal(1, resumed.CachedTiles);
        Assert.Equal(0, resumed.DownloadedTiles);
        Assert.Single(provider.Requests);
    }

    [Fact]
    public async Task ProviderNullAndFailure_AreProcessedAsFailures()
    {
        using var cache = new PreloadCacheFixture();
        var provider = Provider("failure");
        provider.Download = (key, _) =>
            key.Zoom == 2
                ? Task.FromResult<Tile?>(null)
                : Task.FromException<Tile?>(new IOException("Download failed."));

        var result = await new MapPreloadService(cache.Cache).DownloadAsync(
            Request(provider) with
            {
                MinZoom = 2,
                MaxZoom = 3,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(2, result.ProcessedTiles);
        Assert.Equal(2, result.FailedTiles);
        Assert.Equal(0, result.DownloadedTiles);
        Assert.Equal(0, result.DownloadedBytes);
        Assert.Empty(Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories));
    }

    [Fact]
    public async Task PersistenceFailure_IsNotReportedAsDownloaded()
    {
        var provider = Provider("disk-failure");
        var result = await new MapPreloadService(new FailingCache()).DownloadAsync(
            Request(provider),
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(1, result.FailedTiles);
        Assert.Equal(0, result.DownloadedTiles);
        Assert.Equal(0, result.DownloadedBytes);
    }

    [Fact]
    public async Task ConcurrencyLimit_IsSharedAcrossAllSelectedProviders()
    {
        using var cache = new PreloadCacheFixture();
        var first = Provider("one");
        var second = Provider("two");
        var active = 0;
        var maximum = 0;
        async Task<Tile?> Download(TileKey key, CancellationToken ct)
        {
            var current = Interlocked.Increment(ref active);
            int observed;
            do
            {
                observed = Volatile.Read(ref maximum);
            } while (
                current > observed
                && Interlocked.CompareExchange(ref maximum, current, observed) != observed
            );
            try
            {
                await Task.Delay(40, ct);
                return await CreateTile(key);
            }
            finally
            {
                Interlocked.Decrement(ref active);
            }
        }
        first.Download = Download;
        second.Download = Download;

        var result = await new MapPreloadService(cache.Cache).DownloadAsync(
            Request(first, second) with
            {
                MinZoom = 1,
                MaxZoom = 3,
                Concurrency = 2,
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(6, result.DownloadedTiles);
        Assert.Equal(2, maximum);
        Assert.Equal(0, active);
    }

    [Fact]
    public async Task RequestDelay_IsSharedByWorkersForEachProvider()
    {
        using var cache = new PreloadCacheFixture();
        var provider = Provider("paced");
        var starts = new ConcurrentQueue<long>();
        provider.Download = async (key, _) =>
        {
            starts.Enqueue(Stopwatch.GetTimestamp());
            return await CreateTile(key);
        };

        var result = await new MapPreloadService(cache.Cache).DownloadAsync(
            Request(provider) with
            {
                MinZoom = 1,
                MaxZoom = 5,
                Concurrency = 3,
                RequestDelay = TimeSpan.FromMilliseconds(45),
            },
            cancellationToken: TestContext.Current.CancellationToken
        );

        Assert.Equal(5, result.DownloadedTiles);
        var times = starts.ToArray();
        Assert.Equal(5, times.Length);
        for (var i = 1; i < times.Length; i++)
        {
            Assert.True(
                Stopwatch.GetElapsedTime(times[i - 1], times[i]) >= TimeSpan.FromMilliseconds(40)
            );
        }
    }

    [Fact]
    public async Task CancellationDuringProviderDelay_StartsNoFurtherRequests()
    {
        using var cache = new PreloadCacheFixture();
        using var cancel = CancellationTokenSource.CreateLinkedTokenSource(
            TestContext.Current.CancellationToken
        );
        var provider = Provider("cancel-delay");
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        provider.Download = async (key, _) =>
        {
            started.TrySetResult();
            return await CreateTile(key);
        };
        var service = new MapPreloadService(cache.Cache);
        var download = service.DownloadAsync(
            Request(provider) with
            {
                MinZoom = 1,
                MaxZoom = 5,
                RequestDelay = TimeSpan.FromSeconds(30),
                Concurrency = 3,
            },
            cancellationToken: cancel.Token
        );
        await started.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            TestContext.Current.CancellationToken
        );
        cancel.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            download.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)
        );

        Assert.Single(provider.Requests);
        Assert.Single(Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories));
    }

    private PreloadTestProvider Provider(string id)
    {
        var provider = new PreloadTestProvider(id);
        provider.Download = async (key, _) => await CreateTile(key);
        return provider;
    }

    private Task<Tile> CreateTile(TileKey key) =>
        fixture.Session.Dispatch(
            () => PreloadTestProvider.CreateTile(key),
            TestContext.Current.CancellationToken
        );

    private static MapPreloadRequest Request(params ITileProvider[] providers) =>
        new()
        {
            Center = new GeoPoint(10, 10, 0),
            RadiusKm = 0.001,
            MinZoom = 3,
            MaxZoom = 3,
            Providers = providers,
            RequestDelay = TimeSpan.Zero,
            Concurrency = 2,
        };

    private sealed class InlineProgress(Action<MapPreloadProgress> report)
        : IProgress<MapPreloadProgress>
    {
        public void Report(MapPreloadProgress value) => report(value);
    }

    private sealed class FailingCache : IPersistentTileCache
    {
        public bool Contains(TileKey key) => false;

        public ValueTask SaveAsync(Tile tile, CancellationToken cancellationToken = default) =>
            ValueTask.FromException(new IOException("Disk is full."));
    }
}

internal sealed class PreloadTestProvider(string id, int minZoom = 0, int maxZoom = 19)
    : ITileProvider
{
    public static readonly byte[] TileBytes = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII="
    );
    public TileProviderInfo Info { get; } =
        new()
        {
            Id = id,
            NameCallback = () => id,
            Group = TileProviderGroup.Other,
            MinZoom = minZoom,
            MaxZoom = maxZoom,
        };
    public IMapProjection Projection => WebMercatorProjection.Instance;
    public int TileSize => 256;
    public ConcurrentQueue<TileKey> Requests { get; } = new();
    public Func<TileKey, CancellationToken, Task<Tile?>> Download { get; set; } =
        (_, _) => Task.FromResult<Tile?>(null);

    public Task<Tile?> DownloadAsync(TileKey key, CancellationToken ct)
    {
        Requests.Enqueue(key);
        return Download(key, ct);
    }

    public static Tile CreateTile(TileKey key) =>
        Tile.Create(key, new MemoryStream(TileBytes), TileBytes.Length);
}
