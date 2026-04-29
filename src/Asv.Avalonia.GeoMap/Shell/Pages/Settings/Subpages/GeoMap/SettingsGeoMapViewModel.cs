using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class SettingsGeoMapViewModel : SettingsSubPage, ISettingsGeoMapSubPage
{
    public const string PageId = "geo-map";

    public SettingsGeoMapViewModel()
        : this(NullTreeSubPageContext<SettingsPageViewModel>.Instance, NullMapService.Instance, DesignTime.DialogService, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsGeoMapViewModel(
        ITreeSubPageContext<ISettingsPage> pageContext,
        IMapService mapService,
        IDialogService dialogService,
        ILoggerFactory loggerFactory
    )
        : base(PageId, pageContext)
    {
        MapMode = new MapModeProperty(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MapProvider = new MapProviderProperty(mapService, dialogService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MinMapZoom = new MinMapZoomProperty(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MaxMapZoom = new MaxMapZoomProperty(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public MapProviderProperty MapProvider { get; }
    public MapModeProperty MapMode { get; }
    public MinMapZoomProperty MinMapZoom { get; }
    public MaxMapZoomProperty MaxMapZoom { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return MapProvider;
        yield return MapMode;
        yield return MinMapZoom;
        yield return MaxMapZoom;
    }
}
