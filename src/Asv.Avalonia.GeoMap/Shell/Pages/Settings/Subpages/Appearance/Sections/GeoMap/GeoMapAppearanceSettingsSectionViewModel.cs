using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class GeoMapAppearanceSettingsSectionViewModel
    : RoutableViewModel,
        ISettingsAppearanceSection
{
    public const string PageId = "geo-map";

    public GeoMapAppearanceSettingsSectionViewModel()
        : this(NullMapService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public GeoMapAppearanceSettingsSectionViewModel(
        IMapService mapService,
        ILoggerFactory loggerFactory
    )
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
