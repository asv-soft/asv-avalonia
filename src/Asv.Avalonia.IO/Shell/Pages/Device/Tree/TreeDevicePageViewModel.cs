using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.IO;

public abstract class TreeDevicePageViewModel<TContext, TSubPage>
    : TreePageViewModel<TContext, TSubPage>,
        IDevicePage
    where TContext : class, ITreePageViewModel
    where TSubPage : ITreeSubpage
{
    private readonly DevicePageCore _deviceCore;

    protected TreeDevicePageViewModel(
        string typeId,
        IPageContext context,
        IDeviceManager devices,
        IServiceProvider container,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(typeId, context, container, loggerFactory, dialogService, ext)
    {
        _deviceCore = new DevicePageCore(
            devices,
            loggerFactory.CreateLogger<DevicePageCore>(),
            this
        ).DisposeItWith(Disposable);
        _deviceCore.Init(context.NavArgs);

        _deviceCore
            .OnDeviceInitialized.Subscribe(w =>
                w.WhenDisconnectedToken.Register(() =>
                {
                    Nodes.RemoveAll();
                    SelectedPage.Value?.Dispose();
                    SelectedNode.Value?.Dispose();
                    SelectedPage.Value = null;
                    SelectedNode.Value = null;
                })
            )
            .DisposeItWith(Disposable);

        IsDeviceInitialized = _deviceCore
            .IsDeviceInitialized.ToReadOnlyBindableReactiveProperty()
            .DisposeItWith(Disposable);
    }

    public ReadOnlyReactiveProperty<DeviceWrapper?> Target => _deviceCore.Target;
    public IReadOnlyBindableReactiveProperty<bool> IsDeviceInitialized { get; }
    public Observable<DeviceWrapper> OnDeviceInitialized => _deviceCore.OnDeviceInitialized;
    public Observable<Unit> OnDeviceDisconnecting => _deviceCore.OnDeviceDisconnecting;
    public Observable<Unit> OnDeviceDisconnected => _deviceCore.OnDeviceDisconnected;
}
