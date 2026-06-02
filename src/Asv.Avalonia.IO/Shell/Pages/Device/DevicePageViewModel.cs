using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public abstract class DevicePageViewModel<T> : PageViewModel<T>, IDevicePage
    where T : class, IDevicePage
{
    private readonly DevicePageCore _deviceCore;

    protected DevicePageViewModel(
        string id,
        IPageContext context,
        IDeviceManager devices,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(id, context, loggerFactory, dialogService, ext)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(devices);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(dialogService);
        ArgumentNullException.ThrowIfNull(ext);

        _deviceCore = new DevicePageCore(
            devices,
            loggerFactory.CreateLogger<DevicePageCore>(),
            this
        ).DisposeItWith(Disposable);
        _deviceCore.Init(context.NavArgs);

        IsDeviceInitialized = _deviceCore
            .IsDeviceInitialized.ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
    }

    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _deviceCore.Target;
    public IReadOnlyBindableReactiveProperty<bool> IsDeviceInitialized { get; }
    public Observable<Unit> OnDeviceDisconnecting => _deviceCore.OnDeviceDisconnecting;
    public Observable<Unit> OnDeviceDisconnected => _deviceCore.OnDeviceDisconnected;
}
