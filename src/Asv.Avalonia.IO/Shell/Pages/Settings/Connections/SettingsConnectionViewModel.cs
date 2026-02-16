using System.Collections.Specialized;
using System.Composition;
using Asv.Common;
using Asv.IO;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class SettingsConnectionViewModelConfig
{
    public string SelectedItemId { get; set; } = string.Empty;
}

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel
    : ExtendableViewModel<ISettingsConnectionSubPage>,
        ISettingsConnectionSubPage
{
    public const string SubPageId = "connections";
    public const MaterialIconKind Icon = MaterialIconKind.Connection;

    private readonly INavigationService _navigationService;
    private readonly IContainerHost _containerHost;

    private SettingsConnectionViewModelConfig? _config;

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public IExportInfo Source => IoModule.Instance;
    public NotifyCollectionChangedSynchronizedViewList<IPortViewModel> View { get; }
    public IPortViewModel? SelectedItem
    {
        get;
        set => SetField(ref field, value);
    }

    public SettingsConnectionViewModel()
        : this(
            NullDeviceManager.Instance,
            DesignTime.Navigation,
            NullContainerHost.Instance,
            DesignTime.LoggerFactory,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var source = new ObservableList<IPortViewModel>();
        Observable
            .Timer(TimeSpan.FromSeconds(3))
            .Subscribe(x =>
            {
                source.Add(new SerialPortViewModel { Name = { Value = "Serial name" } });
                source.Add(new TcpPortViewModel { Name = { Value = "TCP Client name" } });
                source.Add(new TcpServerPortViewModel { Name = { Value = "TCP Server name" } });
            });
        View = source.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public SettingsConnectionViewModel(
        IDeviceManager deviceManager,
        INavigationService navigationService,
        IContainerHost containerHost,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(SubPageId, loggerFactory, ext)
    {
        _navigationService = navigationService;
        _containerHost = containerHost;
        ObservableList<IProtocolPort> source = [];
        var sourceSyncView = source.CreateView(CreatePort).DisposeItWith(Disposable);
        sourceSyncView.DisposeMany().DisposeItWith(Disposable);
        sourceSyncView.SetRoutableParent(this).DisposeItWith(Disposable);

        View = sourceSyncView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        View.CollectionChanged -= OnChanged;
        View.CollectionChanged += OnChanged;

        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);

        source.AddRange(deviceManager.Router.Ports);

        deviceManager.Router.PortAdded.Subscribe(x => source.Add(x)).DisposeItWith(Disposable);
        deviceManager.Router.PortRemoved.Subscribe(x => source.Remove(x)).DisposeItWith(Disposable);
        Events.Subscribe(InternalCatchEvent).DisposeItWith(Disposable);
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var menu in Menu)
        {
            yield return menu;
        }

        foreach (var model in View)
        {
            yield return model;
        }
    }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            View.CollectionChanged -= OnChanged;
        }

        base.Dispose(disposing);
    }

    private void OnChanged(object? sender, NotifyCollectionChangedEventArgs viewChangedEvent)
    {
        switch (viewChangedEvent.Action)
        {
            case NotifyCollectionChangedAction.Add:
                SelectedItem = viewChangedEvent.NewItems?[0] as IPortViewModel;
                break;
            case NotifyCollectionChangedAction.Remove:
                if (
                    viewChangedEvent.OldItems != null
                    && viewChangedEvent.OldItems.Contains(SelectedItem)
                )
                {
                    var item = View.FirstOrDefault();
                    _navigationService.GoTo(item?.GetPathFromRoot() ?? this.GetPathFromRoot());
                }
                break;
            case NotifyCollectionChangedAction.Replace:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Reset:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private IPortViewModel CreatePort(IProtocolPort protocolPort)
    {
        if (
            !_containerHost.TryGetExport<IPortViewModel>(
                protocolPort.TypeInfo.Scheme,
                out var viewModel
            )
        )
        {
            viewModel = new PortViewModel().SetRoutableParent(this);
        }

        viewModel.Init(protocolPort);
        return viewModel;
    }

    private ValueTask InternalCatchEvent(IRoutable src, AsyncRoutedEvent<IRoutable> e)
    {
        switch (e)
        {
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg => cfg.SelectedItemId = SelectedItem?.Id.ToString() ?? string.Empty
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<SettingsConnectionViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                        SelectedItem = View.FirstOrDefault(x =>
                            x.Id.ToString() == cfg.SelectedItemId
                        )
                );
                break;
        }

        return ValueTask.CompletedTask;
    }
}
