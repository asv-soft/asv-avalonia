using System.Collections.Specialized;
using System.Diagnostics;
using Asv.Common;
using Asv.IO;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia.IO;

/// <summary>
/// Class represents the core functionality of the DevicePage.
/// </summary>
/// <remarks>
/// This class is used for aggregation.
/// </remarks>
public sealed class DevicePageCore : IDisposable
{
    private readonly CompositeDisposable _disposable = new();
    private readonly SynchronizedReactiveProperty<bool> _isDeviceInitialized = new();
    private readonly SynchronizedReactiveProperty<DeviceWrapper?> _target = new();
    private readonly Subject<Unit> _onDeviceDisconnecting = new();
    private readonly Subject<Unit> _onDeviceDisconnected = new();
    private readonly SerialDisposable _waitInitSubscription = new();
    private readonly IDeviceManager _devices;
    private readonly ILayoutService _layoutService;
    private readonly ILogger _logger;
    private readonly IPage _owner;

    private bool _disposed;
    private string? _targetDeviceId;
    private CancellationTokenSource? _deviceDisconnectedToken;

    public DevicePageCore(
        IDeviceManager devices,
        ILayoutService layoutService,
        ILogger logger,
        IPage owner
    )
    {
        if (Design.IsDesignMode)
        {
            _isDeviceInitialized.OnNext(false);
            TimeProvider
                .System.CreateTimer(
                    _ => _isDeviceInitialized.OnNext(true),
                    null,
                    TimeSpan.FromSeconds(5),
                    Timeout.InfiniteTimeSpan
                )
                .DisposeItWith(_disposable);
        }

        _devices = devices;
        _layoutService = layoutService;
        _logger = logger;
        _owner = owner;

        OnDeviceDisconnected = _onDeviceDisconnected.AsObservable();
        OnDeviceDisconnecting = _onDeviceDisconnecting.AsObservable();
    }

    public event Action<IClientDevice, CancellationToken>? OnDeviceInitialized;
    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _target;
    public Observable<Unit> OnDeviceDisconnecting { get; }
    public Observable<Unit> OnDeviceDisconnected { get; }
    public ReadOnlyReactiveProperty<bool> IsDeviceInitialized => _isDeviceInitialized;

    public void Init(NameValueCollection args)
    {
        ThrowIfDisposed();

        _logger.ZLogTrace($"{nameof(_owner.Id)} init args: {args}");
        Debug.Assert(_devices != null, "_devices != null");

        _targetDeviceId =
            args[DevicePageViewModelMixin.ArgsDeviceIdKey]
            ?? throw new ArgumentNullException(
                $"{DevicePageViewModelMixin.ArgsDeviceIdKey} argument is required"
            );

        _onDeviceDisconnecting
            .SubscribeAwait(
                async (_, ct) => await _owner.RequestSaveLayout(_layoutService, ct),
                AwaitOperation.Switch
            )
            .DisposeItWith(_disposable);
        _isDeviceInitialized
            .Where(isInit => isInit)
            .SubscribeAwait(
                async (_, ct) => await _owner.RequestLoadLayout(_layoutService, ct),
                AwaitOperation.Switch
            )
            .DisposeItWith(_disposable);
        _onDeviceDisconnected
            .Synchronize()
            .Subscribe(_ => _isDeviceInitialized.Value = false)
            .DisposeItWith(_disposable);

        _devices
            .Explorer.Devices.ObserveAdd()
            .Where(_targetDeviceId, (e, id) => e.Value.Key.AsString() == id)
            .Subscribe(x => DeviceFoundButNotInitialized(x.Value.Value))
            .DisposeItWith(_disposable);

        _devices
            .Explorer.Devices.ObserveRemove()
            .Where(_targetDeviceId, (e, id) => e.Value.Key.AsString() == id)
            .Subscribe(_ =>
            {
                _onDeviceDisconnecting.OnNext(Unit.Default);
                DeviceRemoved();
                _onDeviceDisconnected.OnNext(Unit.Default);
            })
            .DisposeItWith(_disposable);

        foreach (
            var device in _devices.Explorer.Devices.Where(x => x.Key.AsString() == _targetDeviceId)
        )
        {
            DeviceFoundButNotInitialized(device.Value);
        }
    }

    private void DeviceRemoved()
    {
        _logger.ZLogTrace($"{nameof(_owner.Id)}  device removed: {_targetDeviceId}");
        _deviceDisconnectedToken?.Cancel(false);
        _deviceDisconnectedToken?.Dispose();
        _deviceDisconnectedToken = null;
        _waitInitSubscription.Disposable?.Dispose();
    }

    private void DeviceFoundButNotInitialized(IClientDevice device)
    {
        DeviceRemoved();
        _logger.ZLogTrace($"{nameof(_owner.Id)}  device found: {device.Id}");
        _waitInitSubscription.Disposable = device
            .State.Where(x => x == ClientDeviceState.Complete)
            .Take(1)
            .Subscribe(device, DeviceFoundAndInitialized);
    }

    private void DeviceFoundAndInitialized(ClientDeviceState state, IClientDevice device)
    {
        _logger.ZLogTrace($"{nameof(_owner.Id)}  device initialized: {device.Id}");
        try
        {
            _waitInitSubscription.Disposable?.Dispose();
            _deviceDisconnectedToken = new CancellationTokenSource();
            OnDeviceInitialized?.Invoke(device, _deviceDisconnectedToken.Token);
            _target.OnNext(new DeviceWrapper(device, _deviceDisconnectedToken.Token));
            _isDeviceInitialized.Value = true;
        }
        catch (Exception e)
        {
            _logger.ZLogError(e, $"Error while initializing device {device.Id}");
            throw;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        DeviceRemoved();
        _target.Dispose();
        _disposable.Dispose();
        _onDeviceDisconnecting.Dispose();
        _onDeviceDisconnected.Dispose();
        _waitInitSubscription.Dispose();
        _isDeviceInitialized.Dispose();

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (!_disposed)
        {
            return;
        }

        throw new ObjectDisposedException(nameof(DevicePageCore));
    }
}
