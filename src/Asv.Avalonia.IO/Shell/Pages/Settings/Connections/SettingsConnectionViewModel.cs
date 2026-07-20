using System.Collections.Specialized;
using System.Text.Json;
using Asv.Avalonia;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.IO;

public class SettingsConnectionViewModelConfig
{
    public string SelectedItemId { get; set; } = string.Empty;
}

public class SettingsConnectionViewModel
    : ViewModel<ISettingsConnectionSubPage>,
        ISettingsConnectionSubPage
{
    public const string SubPageId = "connections";
    public const MaterialIconKind Icon = MaterialIconKind.Connection;

    private readonly IServiceProvider _containerHost;
    private readonly IDeviceManager _deviceManager;
    private readonly Subject<Unit> _layoutChanged = new();
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoSink;

    private IPortViewModel? _selectedItem;

    public MenuTree MenuView { get; }
    public ObservableList<IMenuItem> Menu { get; } = [];
    public NotifyCollectionChangedSynchronizedViewList<IPortViewModel> View { get; }
    public IPortViewModel? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetField(ref _selectedItem, value))
            {
                _layoutChanged.OnNext(Unit.Default);
            }
        }
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public SettingsConnectionViewModel()
        : this(
            NullTreeSubPageContext<SettingsPageViewModel>.Instance,
            NullDeviceManager.Instance,
            AppHost.Instance.Services,
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
                source.Add(new TcpClientPortViewModel { Name = { Value = "TCP Client name" } });
                source.Add(new TcpServerPortViewModel { Name = { Value = "TCP Server name" } });
            });
        View = source.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public SettingsConnectionViewModel(
        ITreeSubPageContext<ISettingsPage> context,
        IDeviceManager deviceManager,
        IServiceProvider containerHost,
        IExtensionService ext
    )
        : base(SubPageId, context.Args, ext)
    {
        _layoutChanged.DisposeItWith(Disposable);
        _deviceManager = deviceManager;
        _undoSink = Undo.RegisterValue<string>("default", ApplyPortsSnapshot, ApplyPortsSnapshot)
            .DisposeItWith(Disposable);
        _containerHost = containerHost;
        ObservableList<IProtocolPort> source = [];
        var sourceSyncView = source.CreateView(CreatePort).DisposeItWith(Disposable);
        sourceSyncView.DisposeMany().DisposeItWith(Disposable);
        sourceSyncView.SetParent(this).DisposeItWith(Disposable);

        View = sourceSyncView
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        View.CollectionChanged -= OnChanged;
        View.CollectionChanged += OnChanged;

        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        Menu.SetParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);

        source.AddRange(deviceManager.Router.Ports);

        deviceManager.Router.PortAdded.Subscribe(x => source.Add(x)).DisposeItWith(Disposable);
        deviceManager.Router.PortRemoved.Subscribe(x => source.Remove(x)).DisposeItWith(Disposable);
    }

    public override IEnumerable<IViewModel> GetChildren()
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

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public async ValueTask AddPortAsync(
        ProtocolPortConfig config,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var oldSnapshot = CapturePortsSnapshot();
        await Task.Factory.StartNew(
            () => _deviceManager.Router.AddPort(config.AsUri()),
            cancel,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );
        _undoSink.PublishUpdate(oldSnapshot, CapturePortsSnapshot());
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public async ValueTask UpdatePortAsync(
        string portId,
        ProtocolPortConfig config,
        CancellationToken cancel = default
    )
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var oldSnapshot = CapturePortsSnapshot();
        await Task.Factory.StartNew(
            () =>
            {
                var port = _deviceManager.Router.Ports.First(x => x.Id == portId);
                _deviceManager.Router.RemovePort(port);
                _deviceManager.Router.AddPort(config.AsUri());
            },
            cancel,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );

        _undoSink.PublishUpdate(oldSnapshot, CapturePortsSnapshot());
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    public async ValueTask RemovePortAsync(string portId, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return;
        }

        var oldSnapshot = CapturePortsSnapshot();
        await Task.Factory.StartNew(
            () =>
            {
                var port = _deviceManager.Router.Ports.First(x => x.Id == portId);
                _deviceManager.Router.RemovePort(port);
            },
            cancel,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default
        );

        _undoSink.PublishUpdate(oldSnapshot, CapturePortsSnapshot());
    }

    protected override void AfterLoadExtensions()
    {
        Layout
            .Register(nameof(SettingsConnectionViewModel), LoadLayout, SaveLayout, _layoutChanged)
            .DisposeItWith(Disposable);
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
                    this.GoTo(item?.GetPathFromRoot() ?? this.GetPathFromRoot())
                        .SafeFireAndForget();
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
        var factory = _containerHost.GetKeyedService<
            ViewModelFactoryDelegate<IPortViewModel, IProtocolPort>
        >(protocolPort.TypeInfo.Scheme);
        var viewModel =
            factory?.Invoke(protocolPort)
            ?? ActivatorUtilities.CreateInstance<PortViewModel>(_containerHost, protocolPort);
        viewModel.SetRoutableParent(this);
        viewModel.Init(protocolPort);
        return viewModel;
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    private string CapturePortsSnapshot()
    {
        return JsonSerializer.Serialize(
            _deviceManager.Router.Ports.Select(x => x.Config.AsUri().ToString())
        );
    }

    [System.Diagnostics.CodeAnalysis.RequiresUnreferencedCode(
        "Uses System.Text.Json reflection-based serialization, which is not trim safe."
    )]
    private void ApplyPortsSnapshot(string snapshot)
    {
        var portUris = JsonSerializer.Deserialize<string[]>(snapshot) ?? [];
        foreach (var port in _deviceManager.Router.Ports.ToArray())
        {
            _deviceManager.Router.RemovePort(port);
        }

        foreach (var portUri in portUris)
        {
            _deviceManager.Router.AddPort(new Uri(portUri));
        }
    }

    private SettingsConnectionViewModelConfig SaveLayout()
    {
        return new SettingsConnectionViewModelConfig
        {
            SelectedItemId = SelectedItem?.Id.ToString() ?? string.Empty,
        };
    }

    private void LoadLayout(SettingsConnectionViewModelConfig config)
    {
        SelectedItem = View.FirstOrDefault(x => x.Id.ToString() == config.SelectedItemId);
    }
}
