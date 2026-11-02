using System.Globalization;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>Owns one map-area selection and its cancellable preload operation.</summary>
public sealed class MapPreloadDialogViewModel : DialogViewModelBase
{
    public const string DialogId = $"{BaseId}-map-preload";

    private readonly IMapPreloadService _preloadService;
    private CancellationTokenSource? _estimateCancellation;
    private CancellationTokenSource? _downloadCancellation;
    private Task _estimateTask = Task.CompletedTask;
    private Task _downloadTask = Task.CompletedTask;
    private long _estimateVersion;
    private long _runVersion;
    private bool _closing;
    private MapPreloadProgress? _lastResult;

    public MapPreloadDialogViewModel(
        IMapService mapService,
        IMapPreloadService preloadService,
        GeoPoint initialCenter
    )
        : base(DialogId)
    {
        ArgumentNullException.ThrowIfNull(mapService);
        ArgumentNullException.ThrowIfNull(preloadService);
        _preloadService = preloadService;

        InputBindings = (
            mapService.Input?.Bindings.AsObservable() ?? Observable.Return(MapInputBindings.Default)
        )
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty(
                mapService.Input?.Bindings.CurrentValue ?? MapInputBindings.Default
            )
            .DisposeItWith(Disposable);
        Center = Own(new BindableReactiveProperty<GeoPoint>(initialCenter));
        MapCenter = Own(new BindableReactiveProperty<GeoPoint>(initialCenter));
        PreviewProvider = Own(
            new BindableReactiveProperty<ITileProvider>(mapService.CurrentProvider.Value)
        );
        PreviewZoom = Own(
            new BindableReactiveProperty<int>(
                Math.Clamp(
                    12,
                    PreviewProvider.Value.Info.MinZoom,
                    PreviewProvider.Value.Info.MaxZoom
                )
            )
        );
        RadiusKm = Own(new BindableReactiveProperty<decimal?>(5));
        var defaultMaxZoom = Math.Min(15, PreviewProvider.Value.Info.MaxZoom);
        var defaultMinZoom = Math.Max(
            PreviewProvider.Value.Info.MinZoom,
            Math.Min(10, defaultMaxZoom)
        );
        MinZoom = Own(new BindableReactiveProperty<decimal?>(defaultMinZoom));
        MaxZoom = Own(
            new BindableReactiveProperty<decimal?>(Math.Max(defaultMinZoom, defaultMaxZoom))
        );
        RequestDelayMs = Own(new BindableReactiveProperty<decimal?>(200));
        Concurrency = Own(new BindableReactiveProperty<decimal?>(2));
        RadiusPolygon = Own(new BindableReactiveProperty<IList<GeoPoint>>([]));
        IsRunning = Own(new BindableReactiveProperty<bool>(false));
        ParametersEnabled = Own(new BindableReactiveProperty<bool>(true));
        IsEstimating = Own(new BindableReactiveProperty<bool>(false));
        CanDownload = Own(new BindableReactiveProperty<bool>(false));
        EstimatedTiles = Own(new BindableReactiveProperty<long>(0));
        ProgressPercent = Own(new BindableReactiveProperty<double>(0));
        StatusText = Own(new BindableReactiveProperty<string>(MapPreloadStrings.Ready));
        EstimateText = Own(new BindableReactiveProperty<string>(string.Empty));
        ProgressText = Own(new BindableReactiveProperty<string>(string.Empty));
        StatisticsText = Own(new BindableReactiveProperty<string>(string.Empty));
        ErrorText = Own(new BindableReactiveProperty<string?>(null));
        CenterText = Own(new BindableReactiveProperty<string>(string.Empty));

        Providers = mapService
            .AvailableProviders.Where(provider => provider.Info.Id != EmptyTileProvider.Id)
            .DistinctBy(provider => provider.Info.Id)
            .Select(provider =>
                new MapPreloadProviderViewModel(
                    provider,
                    provider.Info.Id == mapService.CurrentProvider.Value.Info.Id
                )
                    .SetRoutableParent(this)
                    .DisposeItWith(Disposable)
            )
            .ToArray();

        DownloadCommand = new ReactiveCommand(async (_, _) => await DownloadAsync()).DisposeItWith(
            Disposable
        );
        StopCommand = new ReactiveCommand(_ => Stop()).DisposeItWith(Disposable);

        Center.Skip(1).Subscribe(_ => ParametersChanged()).DisposeItWith(Disposable);
        RadiusKm.Skip(1).Subscribe(_ => ParametersChanged()).DisposeItWith(Disposable);
        MinZoom.Skip(1).Subscribe(_ => ScheduleEstimate()).DisposeItWith(Disposable);
        MaxZoom.Skip(1).Subscribe(_ => ScheduleEstimate()).DisposeItWith(Disposable);
        RequestDelayMs.Skip(1).Subscribe(_ => ScheduleEstimate()).DisposeItWith(Disposable);
        Concurrency.Skip(1).Subscribe(_ => ScheduleEstimate()).DisposeItWith(Disposable);
        foreach (var provider in Providers)
        {
            provider
                .IsSelected.Skip(1)
                .Subscribe(_ =>
                {
                    if (!IsRunning.Value)
                    {
                        PreviewProvider.Value =
                            Providers.FirstOrDefault(item => item.IsSelected.Value)?.Provider
                            ?? PreviewProvider.Value;
                        PreviewZoom.Value = Math.Clamp(
                            PreviewZoom.Value,
                            PreviewProvider.Value.Info.MinZoom,
                            PreviewProvider.Value.Info.MaxZoom
                        );
                        ScheduleEstimate();
                    }
                })
                .DisposeItWith(Disposable);
        }

        ParametersChanged();
    }

    public IReadOnlyList<MapPreloadProviderViewModel> Providers { get; }
    public IReadOnlyBindableReactiveProperty<MapInputBindings> InputBindings { get; }
    public BindableReactiveProperty<GeoPoint> Center { get; }
    public BindableReactiveProperty<GeoPoint> MapCenter { get; }
    public BindableReactiveProperty<ITileProvider> PreviewProvider { get; }
    public BindableReactiveProperty<int> PreviewZoom { get; }
    public BindableReactiveProperty<decimal?> RadiusKm { get; }
    public BindableReactiveProperty<decimal?> MinZoom { get; }
    public BindableReactiveProperty<decimal?> MaxZoom { get; }
    public BindableReactiveProperty<decimal?> RequestDelayMs { get; }
    public BindableReactiveProperty<decimal?> Concurrency { get; }
    public BindableReactiveProperty<IList<GeoPoint>> RadiusPolygon { get; }
    public BindableReactiveProperty<bool> IsRunning { get; }
    public BindableReactiveProperty<bool> ParametersEnabled { get; }
    public BindableReactiveProperty<bool> IsEstimating { get; }
    public BindableReactiveProperty<bool> CanDownload { get; }
    public BindableReactiveProperty<long> EstimatedTiles { get; }
    public BindableReactiveProperty<double> ProgressPercent { get; }
    public BindableReactiveProperty<string> StatusText { get; }
    public BindableReactiveProperty<string> EstimateText { get; }
    public BindableReactiveProperty<string> ProgressText { get; }
    public BindableReactiveProperty<string> StatisticsText { get; }
    public BindableReactiveProperty<string?> ErrorText { get; }
    public BindableReactiveProperty<string> CenterText { get; }
    public ReactiveCommand DownloadCommand { get; }
    public ReactiveCommand StopCommand { get; }
    public MapPreloadProgress? LastResult => Volatile.Read(ref _lastResult);

    public override IEnumerable<IViewModel> GetChildren() => Providers;

    public void SelectCenter(GeoPoint center)
    {
        if (!_closing && !IsDisposed && !IsRunning.Value)
        {
            Center.Value = center;
        }
    }

    /// <summary>Starts one immutable request; a subsequent run reuses the shared cache.</summary>
    public Task DownloadAsync()
    {
        if (
            _closing
            || IsDisposed
            || !CanDownload.Value
            || IsRunning.Value
            || !TryCreateRequest(out var request)
        )
        {
            return Task.CompletedTask;
        }

        _downloadTask = DownloadCoreAsync(request);
        return _downloadTask;
    }

    public void Stop()
    {
        if (_closing || IsDisposed || !IsRunning.Value)
        {
            return;
        }

        StatusText.Value = MapPreloadStrings.Stopping;
        _downloadCancellation?.Cancel();
    }

    /// <summary>Cancels and awaits active work before any bound state is disposed.</summary>
    public async Task CancelAndWaitAsync()
    {
        _closing = true;
        _estimateCancellation?.Cancel();
        _downloadCancellation?.Cancel();
        await Task.WhenAll(_estimateTask, _downloadTask);
    }

    private T Own<T>(T value)
        where T : IDisposable => value.DisposeItWith(Disposable);

    private void ParametersChanged()
    {
        if (_closing || IsDisposed || IsRunning.Value)
        {
            return;
        }

        CenterText.Value = string.Format(
            CultureInfo.CurrentCulture,
            MapPreloadStrings.CenterFormat,
            Center.Value.Latitude,
            Center.Value.Longitude
        );
        var radius = (double)(RadiusKm.Value ?? 0);
        RadiusPolygon.Value =
            radius > 0 ? MapPreloadTilePlanner.CreateBoundary(Center.Value, radius).ToArray() : [];
        ScheduleEstimate();
    }

    private bool TryCreateRequest(out MapPreloadRequest request)
    {
        request = null!;
        var providers = Providers
            .Where(provider => provider.IsSelected.Value)
            .Select(provider => provider.Provider)
            .ToArray();
        if (
            providers.Length == 0
            || RadiusKm.Value is not { } radius
            || radius is <= 0 or > 20000
            || MinZoom.Value is not { } minZoom
            || MaxZoom.Value is not { } maxZoom
            || minZoom < 0
            || maxZoom < minZoom
            || maxZoom > 30
            || minZoom != decimal.Truncate(minZoom)
            || maxZoom != decimal.Truncate(maxZoom)
            || RequestDelayMs.Value is not { } delay
            || delay is < 0 or > 60000
            || Concurrency.Value is not { } concurrency
            || concurrency is < 1 or > 16
            || concurrency != decimal.Truncate(concurrency)
            || !double.IsFinite(Center.Value.Latitude)
            || Math.Abs(Center.Value.Latitude) > 90
            || !double.IsFinite(Center.Value.Longitude)
            || Math.Abs(Center.Value.Longitude) > 180
        )
        {
            return false;
        }

        request = new MapPreloadRequest
        {
            Center = Center.Value,
            RadiusKm = (double)radius,
            MinZoom = (int)minZoom,
            MaxZoom = (int)maxZoom,
            Providers = Array.AsReadOnly(providers),
            RequestDelay = TimeSpan.FromMilliseconds((double)delay),
            Concurrency = (int)concurrency,
        };
        return true;
    }

    private void ScheduleEstimate()
    {
        if (_closing || IsDisposed || IsRunning.Value)
        {
            return;
        }

        _estimateCancellation?.Cancel();
        var version = ++_estimateVersion;
        CanDownload.Value = false;
        ErrorText.Value = null;
        EstimatedTiles.Value = 0;
        EstimateText.Value = string.Empty;
        if (!TryCreateRequest(out var request))
        {
            IsEstimating.Value = false;
            StatusText.Value = MapPreloadStrings.InvalidParameters;
            return;
        }

        IsEstimating.Value = true;
        StatusText.Value = MapPreloadStrings.Estimating;
        var cancellation = CancellationTokenSource.CreateLinkedTokenSource(DisposeCancel);
        _estimateCancellation = cancellation;
        _estimateTask = EstimateAsync(request, version, cancellation);
    }

    private async Task EstimateAsync(
        MapPreloadRequest request,
        long version,
        CancellationTokenSource cancellation
    )
    {
        try
        {
            await Task.Delay(300, cancellation.Token).ConfigureAwait(false);
            var count = await Task.Run(
                    () => _preloadService.CountTiles(request, cancellation.Token),
                    cancellation.Token
                )
                .ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (
                    _closing
                    || IsDisposed
                    || version != _estimateVersion
                    || cancellation.IsCancellationRequested
                )
                {
                    return;
                }

                EstimatedTiles.Value = count;
                EstimateText.Value = string.Format(
                    CultureInfo.CurrentCulture,
                    MapPreloadStrings.EstimateFormat,
                    count
                );
                IsEstimating.Value = false;
                CanDownload.Value = count > 0;
                StatusText.Value = MapPreloadStrings.Ready;
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
        catch (Exception exception)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_closing && !IsDisposed && version == _estimateVersion)
                {
                    IsEstimating.Value = false;
                    StatusText.Value = string.Empty;
                    ErrorText.Value = exception.Message;
                }
            });
        }
        finally
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (ReferenceEquals(_estimateCancellation, cancellation))
                {
                    _estimateCancellation = null;
                }
            });
            cancellation.Dispose();
        }
    }

    private async Task DownloadCoreAsync(MapPreloadRequest request)
    {
        IsRunning.Value = true;
        ParametersEnabled.Value = false;
        CanDownload.Value = false;
        ErrorText.Value = null;
        StatusText.Value = MapPreloadStrings.Downloading;
        var version = ++_runVersion;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(DisposeCancel);
        _downloadCancellation = cancellation;
        var initialProgress = new MapPreloadProgress(EstimatedTiles.Value, 0, 0, 0, 0, 0);
        Volatile.Write(ref _lastResult, initialProgress);
        ApplyProgress(initialProgress);
        var progress = new DownloadProgress(this, version);
        try
        {
            var result = await Task.Run(
                () => _preloadService.DownloadAsync(request, progress, cancellation.Token),
                cancellation.Token
            );
            Volatile.Write(ref _lastResult, result);
            if (!_closing && !IsDisposed)
            {
                ApplyProgress(result);
                StatusText.Value = cancellation.IsCancellationRequested
                    ? MapPreloadStrings.Stopped
                    : MapPreloadStrings.Completed;
            }
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            if (!_closing && !IsDisposed)
            {
                StatusText.Value = MapPreloadStrings.Stopped;
            }
        }
        catch (Exception exception)
        {
            if (!_closing && !IsDisposed)
            {
                StatusText.Value = string.Empty;
                ErrorText.Value = exception.Message;
            }
        }
        finally
        {
            _downloadCancellation = null;
            if (!_closing && !IsDisposed)
            {
                IsRunning.Value = false;
                ParametersEnabled.Value = true;
                CanDownload.Value = EstimatedTiles.Value > 0;
            }
        }
    }

    private void ApplyProgress(MapPreloadProgress progress)
    {
        ProgressPercent.Value =
            progress.TotalTiles > 0
                ? Math.Clamp(progress.ProcessedTiles * 100.0 / progress.TotalTiles, 0, 100)
                : 0;
        ProgressText.Value = string.Format(
            CultureInfo.CurrentCulture,
            MapPreloadStrings.ProgressFormat,
            progress.ProcessedTiles,
            progress.TotalTiles,
            ProgressPercent.Value
        );
        StatisticsText.Value = string.Format(
            CultureInfo.CurrentCulture,
            MapPreloadStrings.StatisticsFormat,
            progress.CachedTiles,
            progress.DownloadedTiles,
            progress.FailedTiles,
            progress.DownloadedBytes / 1000000.0
        );
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _closing = true;
            _estimateCancellation?.Cancel();
            _downloadCancellation?.Cancel();
        }

        base.Dispose(disposing);
    }

    private sealed class DownloadProgress(MapPreloadDialogViewModel owner, long version)
        : IProgress<MapPreloadProgress>
    {
        private readonly object _gate = new();
        private MapPreloadProgress? _latest;
        private bool _dispatchPending;

        public void Report(MapPreloadProgress value)
        {
            lock (_gate)
            {
                if (owner.IsDisposed || Volatile.Read(ref owner._runVersion) != version)
                {
                    return;
                }

                Volatile.Write(ref owner._lastResult, value);
                _latest = value;
                if (_dispatchPending)
                {
                    return;
                }

                _dispatchPending = true;
                Dispatcher.UIThread.Post(ApplyLatest);
            }
        }

        private void ApplyLatest()
        {
            MapPreloadProgress? value;
            lock (_gate)
            {
                value = _latest;
                _dispatchPending = false;
            }

            if (
                value is not null
                && !owner._closing
                && !owner.IsDisposed
                && owner.IsRunning.Value
                && owner._runVersion == version
            )
            {
                owner.ApplyProgress(value);
            }
        }
    }
}

/// <summary>A selectable provider owned by the preload dialog.</summary>
public sealed class MapPreloadProviderViewModel : ViewModel
{
    public MapPreloadProviderViewModel(ITileProvider provider, bool isSelected)
        : base($"map-preload-provider-{provider.Info.Id}")
    {
        Provider = provider;
        IsSelected = new BindableReactiveProperty<bool>(isSelected).DisposeItWith(Disposable);
    }

    public ITileProvider Provider { get; }
    public string Name => Provider.Info.Name;
    public BindableReactiveProperty<bool> IsSelected { get; }
}
