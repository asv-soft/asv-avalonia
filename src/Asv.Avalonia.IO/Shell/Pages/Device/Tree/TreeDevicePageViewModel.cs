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
        IServiceProvider container,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(id, cmd, container, layoutService, loggerFactory, dialogService, ext)
    {
        _deviceCore = new DevicePageCore(devices, layoutService, Logger, this);
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitialized;
        _deviceCore.OnDeviceInitialized -= AfterDeviceInitializedBase;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitializedBase;
        _deviceCore.OnDeviceInitialized += AfterDeviceInitialized;

        IsDeviceInitialized = _deviceCore
            .IsDeviceInitialized.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
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
        }

        base.Dispose(disposing);
    }

    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _deviceCore.Target;
    public IReadOnlyBindableReactiveProperty<bool> IsDeviceInitialized { get; }
    public Observable<Unit> OnDeviceDisconnecting => _deviceCore.OnDeviceDisconnecting;
    public Observable<Unit> OnDeviceDisconnected => _deviceCore.OnDeviceDisconnected;
}
