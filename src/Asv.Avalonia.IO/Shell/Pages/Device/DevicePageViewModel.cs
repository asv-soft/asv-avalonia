using System.Collections.Specialized;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public abstract class DevicePageViewModel<T> : PageViewModel<T>, IDevicePage
    where T : class, IDevicePage
{
    private readonly DevicePageCore _deviceCore;

    protected DevicePageViewModel(
        NavigationId id,
        IDeviceManager devices,
        ICommandService cmd,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService
    )
        : base(id, cmd, loggerFactory, dialogService)
    {
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(layoutService);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(cmd);

        _deviceCore = new DevicePageCore(devices, layoutService, Logger, this);
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitialized;

        IsDeviceInitialized = _deviceCore
            .IsDeviceInitialized.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
    }

    protected override void InternalInitArgs(NameValueCollection args)
    {
        _deviceCore.Init(args);
    }

    protected abstract void AfterDeviceInitialized(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    );

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
            _deviceCore.Dispose();
        }

        base.Dispose(disposing);
    }

    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _deviceCore.Target;
    public IReadOnlyBindableReactiveProperty<bool> IsDeviceInitialized { get; }
    public Observable<Unit> OnDeviceDisconnecting => _deviceCore.OnDeviceDisconnecting;
    public Observable<Unit> OnDeviceDisconnected => _deviceCore.OnDeviceDisconnected;
}
