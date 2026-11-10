using System.Diagnostics;

namespace Asv.Avalonia.GeoMap;

public sealed class MapPreloadService(IPersistentTileCache cache) : IMapPreloadService
{
    public long CountTiles(MapPreloadRequest request, CancellationToken cancellationToken = default)
    {
        long count = 0;
        foreach (var tile in MapPreloadTilePlanner.EnumerateTiles(request, cancellationToken))
        {
            count = checked(count + 1);
        }

        return count;
    }

    public async Task<MapPreloadProgress> DownloadAsync(
        MapPreloadRequest request,
        IProgress<MapPreloadProgress>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        MapPreloadTilePlanner.Validate(request);
        request = request with { Providers = request.Providers.ToArray() };

        // Counting and enumeration can cover millions of tiles without occupying the UI thread.
        var total = await Task.Run(() => CountTiles(request, cancellationToken), cancellationToken)
            .ConfigureAwait(false);
        var state = new MapPreloadProgress(total, 0, 0, 0, 0, 0);
        var stateLock = new Lock();
        var enumerationLock = new Lock();
        using var tiles = MapPreloadTilePlanner
            .EnumerateTiles(request, cancellationToken)
            .GetEnumerator();
        var pacers = request
            .Providers.DistinctBy(
                provider => provider.Info.Id,
                StringComparer.InvariantCultureIgnoreCase
            )
            .ToDictionary(
                provider => provider.Info.Id,
                _ => new ProviderPacer(),
                StringComparer.InvariantCultureIgnoreCase
            );
        progress?.Report(state);
        try
        {
            var workers = new Task[request.Concurrency];
            for (var i = 0; i < workers.Length; i++)
            {
                workers[i] = Task.Run(DownloadWorkerAsync, CancellationToken.None);
            }

            await Task.WhenAll(workers).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            Report(null, 0, 0, 0, 0);
            return state;
        }
        finally
        {
            foreach (var pacer in pacers.Values)
            {
                pacer.Dispose();
            }
        }

        async Task DownloadWorkerAsync()
        {
            while (true)
            {
                TileKey key;
                lock (enumerationLock)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!tiles.MoveNext())
                    {
                        return;
                    }

                    key = tiles.Current;
                }

                try
                {
                    if (cache.Contains(key))
                    {
                        Report(key.Provider.Info.Name, 1, 0, 0, 0);
                        continue;
                    }

                    using var tile = await pacers[key.Provider.Info.Id]
                        .DownloadAsync(key, request.RequestDelay, cancellationToken)
                        .ConfigureAwait(false);
                    if (tile == null)
                    {
                        Report(key.Provider.Info.Name, 0, 0, 1, 0);
                        continue;
                    }

                    // A returned download must survive cancellation or closing the dialog.
                    await cache.SaveAsync(tile, CancellationToken.None).ConfigureAwait(false);
                    Report(key.Provider.Info.Name, 0, 1, 0, tile.CompressedSize);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    Report(key.Provider.Info.Name, 0, 0, 1, 0);
                }
            }
        }

        void Report(string? provider, long cached, long downloaded, long failed, long bytes)
        {
            lock (stateLock)
            {
                state = state with
                {
                    ProcessedTiles = state.ProcessedTiles + cached + downloaded + failed,
                    CachedTiles = state.CachedTiles + cached,
                    DownloadedTiles = state.DownloadedTiles + downloaded,
                    FailedTiles = state.FailedTiles + failed,
                    DownloadedBytes = state.DownloadedBytes + bytes,
                    CurrentProvider = provider,
                };
                progress?.Report(state);
            }
        }
    }

    private sealed class ProviderPacer : IDisposable
    {
        private readonly SemaphoreSlim _gate = new(1, 1);
        private long? _lastStart;

        public async Task<Tile?> DownloadAsync(
            TileKey key,
            TimeSpan delay,
            CancellationToken cancellationToken
        )
        {
            Task<Tile?> download;
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (_lastStart.HasValue)
                {
                    var remaining = delay - Stopwatch.GetElapsedTime(_lastStart.Value);
                    while (remaining > TimeSpan.Zero)
                    {
                        await Task.Delay(remaining, cancellationToken).ConfigureAwait(false);
                        remaining = delay - Stopwatch.GetElapsedTime(_lastStart.Value);
                    }
                }

                cancellationToken.ThrowIfCancellationRequested();
                _lastStart = Stopwatch.GetTimestamp();
                download = key.Provider.DownloadAsync(key, cancellationToken);
            }
            finally
            {
                _gate.Release();
            }

            return await download.ConfigureAwait(false);
        }

        public void Dispose() => _gate.Dispose();
    }
}
