using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapModeProperty : RoutableViewModel
{
    public const string ViewModelId = "map.mode";

    private readonly IMapService _mapService;
    private bool _internalChange;

    public MapModeProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapModeProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId, loggerFactory)
    {
        _mapService = mapService;
        SelectedItem = new BindableReactiveProperty<MapModeInfo>().DisposeItWith(Disposable);

        _internalChange = true;
        SelectedItem
            .SubscribeAwait(OnChangedByUser, AwaitOperation.Switch)
            .DisposeItWith(Disposable);
        _mapService.Mode.Synchronize().Subscribe(OnChangeByModel).DisposeItWith(Disposable);
        _internalChange = false;
    }

    public IEnumerable<MapModeInfo> Items =>
        [
            new("Online", MapModeType.Online),
            new("Offline", MapModeType.Offline),
            new("Mixed", MapModeType.Mixed),
        ];

    public BindableReactiveProperty<MapModeInfo> SelectedItem { get; }

    private async ValueTask OnChangedByUser(MapModeInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return;
        }

        _internalChange = true;
        var newValue = new StringArg(userValue.Type.ToString());
        await this.ExecuteCommand(ChangeMapModeCommand.Id, newValue, cancel: cancel);
        _internalChange = false;
    }

    private void OnChangeByModel(MapModeType modelValue)
    {
        _internalChange = true;
        var value = Items.First(info => info.Type == modelValue);
        SelectedItem.Value = value;
        _internalChange = false;
    }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }
}
