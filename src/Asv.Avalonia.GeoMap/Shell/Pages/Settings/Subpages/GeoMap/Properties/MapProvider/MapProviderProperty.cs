using System.Windows.Input;
using Asv.Common;
using Asv.IO;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapProviderProperty : RoutableViewModel
{
    public const string ViewModelId = "map-provider";

    private readonly IMapService _mapService;
    private readonly EditApiKeyDialogPrefab _editApiKeyDialog;
    private readonly ISynchronizedView<ITileProvider, TileProviderViewModel> _view;

    public MapProviderProperty()
        : this(NullMapService.Instance, DesignTime.DialogService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapProviderProperty(
        IMapService mapService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(ViewModelId, loggerFactory)
    {
        _mapService = mapService;
        _editApiKeyDialog = dialogService.GetDialogPrefab<EditApiKeyDialogPrefab>();

        var itemsSource = new ObservableList<ITileProvider>(
            mapService.AvailableProviders.OrderBy(p => p.Info.Group.Id)
        );
        _view = itemsSource
            .CreateView(p => new TileProviderViewModel(p, loggerFactory))
            .DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        SelectedItem = new BindableReactiveProperty<TileProviderViewModel?>(null).DisposeItWith(
            Disposable
        );

        CurrentProvider = new BindableReactiveProperty<TileProviderViewModel?>(
            _view.FirstOrDefault(vm => vm.Provider == mapService.CurrentProvider.Value)
        ).DisposeItWith(Disposable);

        IsSetCurrentEnabled = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);
        IsEditApiKeyEnabled = new BindableReactiveProperty<bool>(false).DisposeItWith(Disposable);

        SetCurrentCommand = new ReactiveCommand(SetCurrentAsync).DisposeItWith(Disposable);
        EditApiKeyCommand = new ReactiveCommand(EditApiKeyAsync).DisposeItWith(Disposable);

        SelectedItem
            .ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateButtonStates())
            .DisposeItWith(Disposable);

        _mapService
            .CurrentProvider.ObserveOnUIThreadDispatcher()
            .Subscribe(provider =>
            {
                CurrentProvider.Value = _view.FirstOrDefault(vm => vm.Provider == provider);
                UpdateButtonStates();
            })
            .DisposeItWith(Disposable);
    }

    public INotifyCollectionChangedSynchronizedViewList<TileProviderViewModel> Items { get; }
    public BindableReactiveProperty<TileProviderViewModel?> SelectedItem { get; }
    public BindableReactiveProperty<TileProviderViewModel?> CurrentProvider { get; }
    public BindableReactiveProperty<bool> IsSetCurrentEnabled { get; }
    public BindableReactiveProperty<bool> IsEditApiKeyEnabled { get; }
    public ICommand SetCurrentCommand { get; }
    public ICommand EditApiKeyCommand { get; }

    private void UpdateButtonStates()
    {
        if (SelectedItem.Value == null)
        {
            IsSetCurrentEnabled.Value = false;
            IsEditApiKeyEnabled.Value = false;
            return;
        }

        IsSetCurrentEnabled.Value = SelectedItem.Value != CurrentProvider.Value;
        IsEditApiKeyEnabled.Value = SelectedItem.Value.Provider is IProtectedTileProvider;
    }

    private async ValueTask SetCurrentAsync(Unit unit, CancellationToken cancel)
    {
        if (SelectedItem.Value == null)
        {
            return;
        }

        var newValue = new StringArg(SelectedItem.Value.Provider.Info.Id);
        await this.ExecuteCommand(ChangeTileProviderCommand.Id, newValue, cancel);
    }

    private async ValueTask EditApiKeyAsync(Unit unit, CancellationToken cancel)
    {
        if (SelectedItem.Value is not { Provider: IProtectedTileProvider })
        {
            return;
        }

        var currentKey = _mapService.GetProviderApiKey(SelectedItem.Value.Provider.Info.Id);
        var result = await _editApiKeyDialog.ShowDialogAsync(
            new EditApiKeyDialogPayload { CurrentApiKey = currentKey }
        );

        if (result != currentKey)
        {
            _mapService.SetProviderApiKey(SelectedItem.Value.Provider.Info.Id, result);
        }
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        foreach (var tileProvider in _view)
        {
            yield return tileProvider;
        }
    }
}
