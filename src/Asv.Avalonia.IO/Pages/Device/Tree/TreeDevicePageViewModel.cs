using System.Collections.Specialized;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public abstract class TreeDevicePageViewModel<TContext, TSubPage>
    : TreePageViewModel<TContext, TSubPage>,
        IDevicePage
    where TContext : class, IPage
    where TSubPage : ITreeSubpage<TContext>
{
    private readonly DevicePageCore _deviceCore;

    protected TreeDevicePageViewModel(
        NavigationId id,
        IDeviceManager devices,
        ICommandService cmd,
        IContainerHost container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(id, cmd, container, layoutService, loggerFactory)
    {
        _deviceCore = new DevicePageCore(devices, layoutService, Logger, this);
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitializedBase;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitializedBase;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitialized;
        _deviceCore.DisposeItWith(Disposable);
    }

    private void AfterDeviceInitializedBase(
        IClientDevice device,
        CancellationToken onDisconnectedToken
    )
    {
        onDisconnectedToken.Register(() =>
        {
            Nodes.RemoveAll();
            SelectedPage.Value?.Dispose();
            SelectedNode.Value?.Dispose();
            SelectedPage.Value = null;
            SelectedNode.Value = null;
        });
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
            _deviceCore.OnDeviceInitialized -= AfterDeviceInitializedBase;
            _deviceCore.Dispose();
            Target.Dispose();
            IsDeviceInitialized.Dispose();
        }

        base.Dispose(disposing);
    }

    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _deviceCore.Target;
    public ReadOnlyReactiveProperty<bool> IsDeviceInitialized => _deviceCore.IsDeviceInitialized;
    public Observable<Unit> OnDeviceDisconnecting => _deviceCore.OnDeviceDisconnecting;
    public Observable<Unit> OnDeviceDisconnected => _deviceCore.OnDeviceDisconnected;
}
