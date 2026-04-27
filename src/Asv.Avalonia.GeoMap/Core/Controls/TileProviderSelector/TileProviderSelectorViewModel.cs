using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileProviderSelectorViewModel : ViewModel
{
    public const string ViewModelId = "tile-provider";

    private readonly IMapService _mapService;
    private readonly ISynchronizedView<ITileProvider, TileProviderViewModel> _view;
    private bool _internalChange;

    public TileProviderSelectorViewModel()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TileProviderSelectorViewModel(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId)
    {
        _mapService = mapService;

        var itemsSource = new ObservableList<ITileProvider>(
            mapService.AvailableProviders.OrderBy(p => p.Info.Group.Id)
        );
        _view = itemsSource
            .CreateView(p => new TileProviderViewModel(p))
            .DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        SelectedItem = new BindableReactiveProperty<TileProviderViewModel?>(
            _view.FirstOrDefault(vm => vm.Provider == mapService.CurrentProvider.Value)
        ).DisposeItWith(Disposable);

        _internalChange = true;
        SelectedItem.SubscribeAwait(OnChangedByUser).DisposeItWith(Disposable);
        _mapService
            .CurrentProvider.ObserveOnUIThreadDispatcher()
            .Subscribe(OnChangeByModel)
            .DisposeItWith(Disposable);
        _internalChange = false;
    }

    public INotifyCollectionChangedSynchronizedViewList<TileProviderViewModel> Items { get; }
    public BindableReactiveProperty<TileProviderViewModel?> SelectedItem { get; }

    private async ValueTask OnChangedByUser(
        TileProviderViewModel? userValue,
        CancellationToken cancel
    )
    {
        if (_internalChange || userValue == null)
        {
            return;
        }

        _internalChange = true;
        var newValue = new StringArg(userValue.Provider.Info.Id);
        await this.ExecuteCommand(ChangeTileProviderCommand.Id, newValue, cancel: cancel);
        _internalChange = false;
    }

    private void OnChangeByModel(ITileProvider modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = _view.FirstOrDefault(vm => vm.Provider == modelValue);
        _internalChange = false;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        foreach (var tileProvider in _view)
        {
            yield return tileProvider;
        }
    }
}
