using System.Collections.Specialized;
using Asv.Cfg;
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
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, layoutService, loggerFactory)
    {
        _deviceCore = new DevicePageCore(devices, Logger, this);
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitialized;
        _deviceCore.DisposeItWith(Disposable);
        Target
            .Where(wrapper => wrapper is not null)
            .SubscribeAwait(async (_, ct) => await this.RequestLoadLayout(ct))
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
}
