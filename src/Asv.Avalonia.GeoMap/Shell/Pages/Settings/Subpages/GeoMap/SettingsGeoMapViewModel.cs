using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class SettingsGeoMapViewModel : SettingsSubPage, ISettingsGeoMapSubPage
{
    public const string PageId = "geo-map";

    public SettingsGeoMapViewModel()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SettingsGeoMapViewModel(IMapService mapService, ILoggerFactory loggerFactory)
        : base(PageId, loggerFactory)
    {
        MapMode = new MapModeProperty(mapService, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        MapProvider = new MapProviderProperty(loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public MapProviderProperty MapProvider { get; }
    public MapModeProperty MapMode { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MapProvider;
        yield return MapMode;
    }
}
