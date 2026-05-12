using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

#pragma warning disable SA1313
public record struct MapModeInfo(string Name, MapModeType Type);
#pragma warning restore SA1313

public class MapModeProperty : ViewModel
{
    public const string ViewModelId = "map-mode";

    private readonly IMapService _mapService;
    private readonly IUndoChangeSink<ValueUndoChange<MapModeType>> _undoSink;
    private bool _internalChange;

    public MapModeProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapModeProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId)
    {
        _mapService = mapService;
        _undoSink = Undo.CreateValueChange<MapModeType>("default", ApplyMapMode, ApplyMapMode)
            .DisposeItWith(Disposable);
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
            new(RS.MapModeProperty_MapModeInfo_Online, MapModeType.Online),
            new(RS.MapModeProperty_MapModeInfo_Offline, MapModeType.Offline),
            new(RS.MapModeProperty_MapModeInfo_Mixed, MapModeType.Mixed),
        ];

    public BindableReactiveProperty<MapModeInfo> SelectedItem { get; }

    private ValueTask OnChangedByUser(MapModeInfo userValue, CancellationToken cancel)
    {
        if (_internalChange)
        {
            return ValueTask.CompletedTask;
        }

        var oldValue = _mapService.Mode.Value;
        var newValue = userValue.Type;
        if (oldValue == newValue)
        {
            return ValueTask.CompletedTask;
        }

        try
        {
            _internalChange = true;
            ApplyMapMode(newValue);
            _undoSink.Publish(oldValue, newValue);
            return ValueTask.CompletedTask;
        }
        finally
        {
            _internalChange = false;
        }
    }

    private void OnChangeByModel(MapModeType modelValue)
    {
        _internalChange = true;
        var value = Items.First(info => info.Type == modelValue);
        SelectedItem.Value = value;
        _internalChange = false;
    }

    private void ApplyMapMode(MapModeType value)
    {
        _mapService.Mode.Value = value;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}
