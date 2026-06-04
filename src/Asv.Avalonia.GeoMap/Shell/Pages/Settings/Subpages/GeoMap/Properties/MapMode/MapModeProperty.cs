using Asv.Avalonia;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapModeProperty : PropertyComboBoxViewModel
{
    public const string ViewModelId = "map-mode";

    private readonly IMapService _mapService;

    public MapModeProperty()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public MapModeProperty(IMapService mapService, ILoggerFactory loggerFactory)
        : base(ViewModelId)
    {
        _mapService = mapService;
        Header = RS.SettingsGeoMapView_ModeProperty_Title;
        Description = RS.SettingsGeoMapView_ModeProperty_Description;
        Icon = MaterialIconKind.LetterMBoxOutline;
        IconColor = AsvColorKind.Info3;

        ItemsSource.Add(
            new MapModeItem(
                RS.MapModeProperty_MapModeInfo_Online,
                RS.MapModeProperty_MapModeInfo_Online_Description,
                MapModeType.Online,
                MaterialIconKind.CloudOutline,
                AsvColorKind.Info4
            )
        );
        ItemsSource.Add(
            new MapModeItem(
                RS.MapModeProperty_MapModeInfo_Offline,
                RS.MapModeProperty_MapModeInfo_Offline_Description,
                MapModeType.Offline,
                MaterialIconKind.DatabaseOutline,
                AsvColorKind.Info8
            )
        );
        ItemsSource.Add(
            new MapModeItem(
                RS.MapModeProperty_MapModeInfo_Mixed,
                RS.MapModeProperty_MapModeInfo_Mixed_Description,
                MapModeType.Mixed,
                MaterialIconKind.CloudSyncOutline,
                AsvColorKind.Info7
            )
        );

        _mapService
            .Mode.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(value => OnChangeByModel(value))
            .DisposeItWith(Disposable);
        OnChangeByModel(_mapService.Mode.Value);
    }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        if (item is not MapModeItem mapMode)
        {
            return ValueTask.CompletedTask;
        }

        _mapService.Mode.Value = mapMode.Type;
        return ValueTask.CompletedTask;
    }

    private void OnChangeByModel(MapModeType modelValue)
    {
        ApplyValueFromModel(
            ItemsSource.OfType<MapModeItem>().First(info => info.Type == modelValue)
        );
    }

    private sealed class MapModeItem : HeadlinedViewModel
    {
        public MapModeItem(
            string header,
            string description,
            MapModeType type,
            MaterialIconKind icon,
            AsvColorKind iconColor
        )
            : base(type.ToString())
        {
            Header = header;
            Description = description;
            Type = type;
            Icon = icon;
            IconColor = iconColor;
        }

        public MapModeType Type { get; }
    }
}
