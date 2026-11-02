using System.Diagnostics.Metrics;
using Asv.Avalonia.GeoMap;
using Asv.Modeling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Asv.Avalonia.Test;

[Collection(WorkspaceUiTestCollection.Name)]
public class FileSystemPreloadCacheTest(WorkspaceUiTestFixture fixture)
{
    [Fact]
    public async Task QueuedWrites_WhenCapacityIsExceeded_PersistEveryTransferredTile()
    {
        using var cache = new PreloadCacheFixture();
        var provider = new PreloadTestProvider("queued");
        var tiles = await fixture.Session.Dispatch(
            () =>
                Enumerable
                    .Range(0, 80)
                    .Select(x => PreloadTestProvider.CreateTile(new TileKey(x, 1, 8, provider)))
                    .ToArray(),
            TestContext.Current.CancellationToken
        );

        foreach (var tile in tiles)
        {
            cache.Cache[tile.Key] = tile;
        }

        await cache.Cache.DisposeAsync();
        Assert.Equal(
            80,
            Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories).Length
        );
    }

    [Fact]
    public async Task DurableSave_ReturnsOnlyAfterOriginalBytesAreReadableAndSurvivesReopen()
    {
        using var cache = new PreloadCacheFixture();
        var key = new TileKey(2, 3, 4, new PreloadTestProvider("durable"));
        using var tile = await fixture.Session.Dispatch(
            () => PreloadTestProvider.CreateTile(key),
            TestContext.Current.CancellationToken
        );

        await cache.Cache.SaveAsync(tile);

        Assert.True(cache.Cache.Contains(key));
        var path = Assert.Single(
            Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories)
        );
        Assert.Equal(PreloadTestProvider.TileBytes, await File.ReadAllBytesAsync(path));
        Assert.Empty(Directory.GetFiles(cache.Path, "*.tmp", SearchOption.AllDirectories));
        await cache.Cache.DisposeAsync();
        using var reopened = cache.CreateCache();
        Assert.True(reopened.Contains(key));
    }

    [Fact]
    public void Contains_ChecksDiskWithoutDecodingOrCreatingProviderDirectories()
    {
        using var cache = new PreloadCacheFixture();
        var key = new TileKey(2, 3, 4, new PreloadTestProvider("exists"));
        Assert.False(cache.Cache.Contains(key));
        Assert.False(Directory.Exists(System.IO.Path.Combine(cache.Path, "exists")));
        var folder = System.IO.Path.Combine(cache.Path, "exists", "4");
        Directory.CreateDirectory(folder);
        File.WriteAllText(
            System.IO.Path.Combine(folder, "2_3.png"),
            "Not an image: existence must not decode it."
        );

        Assert.True(cache.Cache.Contains(key));
    }

    [Fact]
    public async Task DurableAndQueuedWrites_ToSameKey_LeaveOneCompleteFileAndCorrectStatistics()
    {
        using var cache = new PreloadCacheFixture();
        var key = new TileKey(2, 3, 4, new PreloadTestProvider("shared"));
        var queued = await fixture.Session.Dispatch(
            () => PreloadTestProvider.CreateTile(key),
            TestContext.Current.CancellationToken
        );
        using var durable = await fixture.Session.Dispatch(
            () => PreloadTestProvider.CreateTile(key),
            TestContext.Current.CancellationToken
        );
        cache.Cache[key] = queued;
        await cache.Cache.SaveAsync(durable);
        await cache.Cache.DisposeAsync();

        var path = Assert.Single(
            Directory.GetFiles(cache.Path, "*.png", SearchOption.AllDirectories)
        );
        Assert.Equal(PreloadTestProvider.TileBytes, await File.ReadAllBytesAsync(path));
        Assert.Equal(1, cache.Cache.GetStatistic().TileCount);
        Assert.Equal(PreloadTestProvider.TileBytes.Length, cache.Cache.GetStatistic().Size);
    }
}

internal sealed class PreloadCacheFixture : IDisposable, IAppPath
{
    private readonly ServiceProvider _services = new ServiceCollection()
        .AddMetrics()
        .BuildServiceProvider();
    public string Path { get; } =
        System.IO.Path.Combine(
            System.IO.Path.GetTempPath(),
            "asv-preload-" + Guid.NewGuid().ToString("N")
        );
    public FileSystemCache Cache { get; }

    public PreloadCacheFixture() => Cache = CreateCache();

    public FileSystemCache CreateCache() =>
        new(
            Options.Create(
                new FileSystemCacheConfig
                {
                    WriteQueueSize = 1,
                    WriteParallelThreads = 1,
                    PrintStatisticsToLogPeriodSec = 0,
                }
            ),
            NullLoggerFactory.Instance,
            _services.GetRequiredService<IMeterFactory>(),
            this
        );

    public string GetAppPathFolder(string additionalPrefix)
    {
        Directory.CreateDirectory(Path);
        return Path;
    }

    public string GetAppPathFile(string fileName) => System.IO.Path.Combine(Path, fileName);

    public string GetPageFolder(NavId pageId, string additionalPrefix) =>
        throw new NotSupportedException();

    public void Dispose()
    {
        Cache.Dispose();
        _services.Dispose();
        Directory.Delete(Path, true);
    }
}
