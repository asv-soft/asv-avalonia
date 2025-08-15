using System.Collections.Specialized;
using System.Diagnostics;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia.IO;

public abstract class DevicePageViewModel<T, TCfg> : PageViewModel<T, TCfg>, IDevicePage
    where T : class, IDevicePage
    where TCfg : PageConfig, new()
{
    private readonly IDeviceManager? _devices;
    private string? _targetDeviceId;
    private IDisposable? _waitInitSubscription;
    private CancellationTokenSource? _deviceDisconnectedToken;
    private readonly ReactiveProperty<DeviceWrapper?> _target;

    protected DevicePageViewModel(
        NavigationId id,
        IDeviceManager devices,
        ICommandService cmd,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, cfg, loggerFactory)
    {
        _devices = devices;
        _target = new ReactiveProperty<DeviceWrapper?>().DisposeItWith(Disposable);
        Disposable.AddAction(DeviceRemoved);
    }

    protected override void InternalInitArgs(NameValueCollection args)
    {
        Logger.ZLogTrace($"{this} init args: {args}");
        base.InternalInitArgs(args);
        Debug.Assert(_devices != null, "_devices != null");
        _targetDeviceId =
            args[DevicePageViewModelMixin.ArgsDeviceIdKey]
            ?? throw new ArgumentNullException(
                $"{DevicePageViewModelMixin.ArgsDeviceIdKey} argument is required for {GetType().Name} page"
            );
        _devices
            .Explorer.Devices.ObserveAdd()
            .Where(_targetDeviceId, (e, id) => e.Value.Key.AsString() == id)
            .Subscribe(x => DeviceFoundButNotInitialized(x.Value.Value))
            .DisposeItWith(Disposable);

        _devices
            .Explorer.Devices.ObserveRemove()
            .Where(_targetDeviceId, (e, id) => e.Value.Key.AsString() == id)
            .Subscribe(_ => DeviceRemoved())
            .DisposeItWith(Disposable);

        foreach (
            var device in _devices.Explorer.Devices.Where(x => x.Key.AsString() == _targetDeviceId)
        )
        {
            DeviceFoundButNotInitialized(device.Value);
        }
    }

    private void DeviceRemoved()
    {
        Logger.ZLogTrace($"{this} device removed: {_targetDeviceId}");
        _deviceDisconnectedToken?.Cancel(false);
        _deviceDisconnectedToken?.Dispose();
        _deviceDisconnectedToken = null;
        _waitInitSubscription?.Dispose();
        _waitInitSubscription = null;
    }

    private void DeviceFoundButNotInitialized(IClientDevice device)
    {
        DeviceRemoved();
        Logger.ZLogTrace($"{this} device found: {device.Id}");
        _waitInitSubscription = device
            .State.Where(x => x == ClientDeviceState.Complete)
            .Take(1)
            .Subscribe(device, DeviceFoundAndInitialized);
    }

    private void DeviceFoundAndInitialized(ClientDeviceState state, IClientDevice device)
    {
        Logger.ZLogTrace($"{this} device initialized: {device.Id}");
        try
        {
            _waitInitSubscription?.Dispose();
            _deviceDisconnectedToken = new CancellationTokenSource();
            AfterDeviceInitialized(device, _deviceDisconnectedToken.Token);
            _target.OnNext(new DeviceWrapper(device, _deviceDisconnectedToken.Token));
        }
        catch (Exception e)
        {
            Logger.ZLogError(e, $"Error while initializing device {device.Id} in {this}");
            throw;
        }
    }

    protected abstract void AfterDeviceInitialized(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    );
    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _target;
}
