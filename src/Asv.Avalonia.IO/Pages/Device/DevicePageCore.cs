using System.Collections.Specialized;
using System.Diagnostics;
using Asv.Common;
using Asv.IO;
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
/// <param name="devices">The device manager.</param>
/// <param name="logger">Logger of the owner.</param>
/// <param name="owner">Page that wants to extend itself with DevicePage functionality.</param>
public sealed class DevicePageCore(
    IDeviceManager devices,
    ILayoutService layoutService,
    ILogger logger,
    IPage owner
) : IDisposable
{
    private readonly CompositeDisposable _disposable = new();
    private readonly ReactiveProperty<bool> _isDeviceInitialized = new();
    private readonly ReactiveProperty<DeviceWrapper?> _target = new();
    private readonly Subject<Unit> _onDeviceDisconnecting = new();
    private readonly Subject<Unit> _onDeviceDisconnected = new();

    private bool _disposed;
    private string? _targetDeviceId;
    private IDisposable? _waitInitSubscription;
    private CancellationTokenSource? _deviceDisconnectedToken;

    public event Action<IClientDevice, CancellationToken>? OnDeviceInitialized;
    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _target;
    public Observable<Unit> OnDeviceDisconnecting => _onDeviceDisconnecting.AsObservable();
    public Observable<Unit> OnDeviceDisconnected => _onDeviceDisconnected.AsObservable();
    public ReadOnlyReactiveProperty<bool> IsDeviceInitialized => _isDeviceInitialized;

    public void Init(NameValueCollection args)
    {
        ThrowIfDisposed();

        logger.ZLogTrace($"{nameof(owner.Id)} init args: {args}");
        Debug.Assert(devices != null, "_devices != null");

        _targetDeviceId =
            args[DevicePageViewModelMixin.ArgsDeviceIdKey]
            ?? throw new ArgumentNullException(
                $"{DevicePageViewModelMixin.ArgsDeviceIdKey} argument is required"
            );

        _onDeviceDisconnecting
            .SubscribeAwait(async (_, ct) => await owner.RequestSaveLayout(layoutService, ct))
            .DisposeItWith(_disposable);
        _isDeviceInitialized
            .Where(isInit => isInit)
            .SubscribeAwait(async (_, ct) => await owner.RequestLoadLayout(layoutService, ct))
            .DisposeItWith(_disposable);
        _onDeviceDisconnected
            .Subscribe(_ => _isDeviceInitialized.Value = false)
            .DisposeItWith(_disposable);

        devices
            .Explorer.Devices.ObserveAdd()
            .Where(_targetDeviceId, (e, id) => e.Value.Key.AsString() == id)
            .Subscribe(x => DeviceFoundButNotInitialized(x.Value.Value))
            .DisposeItWith(_disposable);

        devices
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
            var device in devices.Explorer.Devices.Where(x => x.Key.AsString() == _targetDeviceId)
        )
        {
            DeviceFoundButNotInitialized(device.Value);
        }
    }

    private void DeviceRemoved()
    {
        logger.ZLogTrace($"{nameof(owner.Id)}  device removed: {_targetDeviceId}");
        _deviceDisconnectedToken?.Cancel(false);
        _deviceDisconnectedToken?.Dispose();
        _deviceDisconnectedToken = null;
        _waitInitSubscription?.Dispose();
        _waitInitSubscription = null;
    }

    private void DeviceFoundButNotInitialized(IClientDevice device)
    {
        DeviceRemoved();
        logger.ZLogTrace($"{nameof(owner.Id)}  device found: {device.Id}");
        _waitInitSubscription = device
            .State.Where(x => x == ClientDeviceState.Complete)
            .Take(1)
            .Subscribe(device, DeviceFoundAndInitialized);
    }

    private void DeviceFoundAndInitialized(ClientDeviceState state, IClientDevice device)
    {
        logger.ZLogTrace($"{nameof(owner.Id)}  device initialized: {device.Id}");
        try
        {
            _waitInitSubscription?.Dispose();
            _deviceDisconnectedToken = new CancellationTokenSource();
            OnDeviceInitialized?.Invoke(device, _deviceDisconnectedToken.Token);
            _target.OnNext(new DeviceWrapper(device, _deviceDisconnectedToken.Token));
            _isDeviceInitialized.Value = true;
        }
        catch (Exception e)
        {
            logger.ZLogError(e, $"Error while initializing device {device.Id}");
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
