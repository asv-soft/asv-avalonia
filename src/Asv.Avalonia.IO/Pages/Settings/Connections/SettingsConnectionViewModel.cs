using System.Collections.Specialized;
using System.Composition;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel
    : ExtendableViewModel<ISettingsConnectionSubPage>,
        ISettingsConnectionSubPage
{
    private readonly IDeviceManager _deviceManager;
    private readonly INavigationService _navigationService;
    private readonly IContainerHost _containerHost;
    private IPortViewModel? _selectedItem;

    public const string SubPageId = "settings.connection1";

    public SettingsConnectionViewModel()
        : this(
            NullDeviceManager.Instance,
            DesignTime.Navigation,
            NullContainerHost.Instance,
            DesignTime.LoggerFactory
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        var source = new ObservableList<IPortViewModel>();
        Observable
            .Timer(TimeSpan.FromSeconds(3))
            .Subscribe(x =>
            {
                source.Add(new SerialPortViewModel() { Name = { Value = "Serial name" } });
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
        ILoggerFactory loggerFactory
    )
        : base(SubPageId, loggerFactory)
    {
        _deviceManager = deviceManager;
        _navigationService = navigationService;
        _containerHost = containerHost;
        ObservableList<IProtocolPort> source = [];
        var sourceSyncView = source.CreateView(CreatePort).DisposeItWith(Disposable);
        sourceSyncView.DisposeMany().DisposeItWith(Disposable);
        sourceSyncView.SetRoutableParent(this).DisposeItWith(Disposable);

        View = sourceSyncView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        View.CollectionChanged += (sender, args) =>
        {
            OnChanged(args);
        };

        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);

        foreach (var port in deviceManager.Router.Ports)
        {
            source.Add(port);
        }

        deviceManager.Router.PortAdded.Subscribe(x => source.Add(x)).DisposeItWith(Disposable);
        deviceManager.Router.PortRemoved.Subscribe(x => source.Remove(x)).DisposeItWith(Disposable);
    }

    private void OnChanged(NotifyCollectionChangedEventArgs viewChangedEvent)
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
                    _navigationService.GoTo(item?.GetPathToRoot() ?? this.GetPathToRoot());
                }
                break;
            case NotifyCollectionChangedAction.Replace:
                break;
            case NotifyCollectionChangedAction.Move:
                break;
            case NotifyCollectionChangedAction.Reset:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        var item = View.FirstOrDefault(x => x.Id == id);
        if (item != null)
        {
            SelectedItem = item;
            return new ValueTask<IRoutable>(item);
        }
        return base.Navigate(id);
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
            viewModel = new PortViewModel { Parent = this };
        }

        viewModel.Init(protocolPort);
        return viewModel;
    }

    public IPortViewModel? SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<IPortViewModel> View { get; }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
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

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public IExportInfo Source => IoModule.Instance;
}
