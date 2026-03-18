using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileProviderSelectorViewModel : RoutableViewModel
{
    public const string ViewModelId = "tile-provider";

    private readonly IMapService _mapService;
    private bool _internalChange;

    public TileProviderSelectorViewModel()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public TileProviderSelectorViewModel(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, loggerFactory)
    {
        _mapService = mapService;
        SelectedItem = new BindableReactiveProperty<ITileProvider>(
            mapService.CurrentProvider.Value
        ).DisposeItWith(Disposable);

        _internalChange = true;
        SelectedItem.SubscribeAwait(OnChangedByUser).DisposeItWith(Disposable);
        _mapService
            .CurrentProvider.Synchronize()
            .Subscribe(OnChangeByModel)
            .DisposeItWith(Disposable);
        _internalChange = false;
    }

    public IReadOnlyList<ITileProvider> Items => _mapService.AvailableProviders;
    public BindableReactiveProperty<ITileProvider> SelectedItem { get; }

    private async ValueTask OnChangedByUser(ITileProvider userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        var newValue = new StringArg(userValue.Info.Id);
        await this.ExecuteCommand(ChangeTileProviderCommand.Id, newValue, cancel: cancel);
        _internalChange = false;
    }

    private void OnChangeByModel(ITileProvider modelValue)
    {
        _internalChange = true;
        SelectedItem.Value = modelValue;
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
