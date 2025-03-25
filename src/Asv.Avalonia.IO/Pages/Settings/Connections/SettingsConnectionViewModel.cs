using System.Collections.Specialized;
using System.Composition;
using Asv.Common;
using Asv.IO;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

[ExportSettings(SubPageId)]
public class SettingsConnectionViewModel
    : ExtendableViewModel<ISettingsConnectionSubPage>,
        ISettingsConnectionSubPage
{
    private readonly IDeviceManager _deviceManager;
    private readonly IContainerHost _containerHost;
    private IPortViewModel? _selectedItem;

    public const string SubPageId = "settings.connection1";

    public SettingsConnectionViewModel()
        : this(NullDeviceManager.Instance, NullContainerHost.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        var port1 = new PortViewModel();
        port1.Name.Value = "Port 1";
        var port2 = new PortViewModel();
        port2.Name.Value = "Port 2";

        var source = new ObservableList<IPortViewModel> { port1, port2 };
        View = source.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    [ImportingConstructor]
    public SettingsConnectionViewModel(IDeviceManager deviceManager, IContainerHost containerHost)
        : base(SubPageId)
    {
        _deviceManager = deviceManager;
        _containerHost = containerHost;
        ObservableList<IProtocolPort> source = [];
        var sourceSyncView = source.CreateView(CreatePort).DisposeItWith(Disposable);
        sourceSyncView.SetRoutableParentForView(this, true).DisposeItWith(Disposable);

        View = sourceSyncView.ToNotifyCollectionChanged().DisposeItWith(Disposable);
        View.CollectionChanged += (sender, args) =>
        {
            OnChanged(args);
        };

        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        Menu.SetRoutableParent(this, true).DisposeItWith(Disposable);

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
                    SelectedItem = View.FirstOrDefault();
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

    private IPortViewModel CreatePort(IProtocolPort protocolPort)
    {
        if (
            !_containerHost.TryGetExport<IPortViewModel>(
                protocolPort.TypeInfo.Scheme,
                out var viewModel
            )
        )
        {
            viewModel = new PortViewModel();
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
