using System.Collections.Specialized;
using Asv.Cfg;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public abstract class TreeDevicePageViewModel<TContext, TSubPage, TConfig>
    : TreePageViewModel<TContext, TSubPage, TConfig>,
        IDevicePage
    where TContext : class, IPage
    where TSubPage : ITreeSubpage<TContext>
    where TConfig : PageConfig, new()
{
    private readonly DevicePageCore _deviceCore;

    protected TreeDevicePageViewModel(
        NavigationId id,
        IDeviceManager devices,
        ICommandService cmd,
        IContainerHost container,
        IConfiguration cfg,
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, container, cfg, loggerFactory)
    {
        _deviceCore = new DevicePageCore(devices, Logger, this);
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitialized;
        _deviceCore.DisposeItWith(Disposable);
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
