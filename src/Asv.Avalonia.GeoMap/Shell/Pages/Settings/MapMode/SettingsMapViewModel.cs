using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public sealed class SettingsMapViewModel : SettingsSubPage
{
    public const string SubPageId = "map";

    public SettingsMapViewModel()
        : this(NullMapService.Instance, DesignTime.LoggerFactory) { }

    public SettingsMapViewModel(IMapService tileLoader, ILoggerFactory loggerFactory)
        : base(SubPageId, loggerFactory)
    {
        MapModeProperty = new MapModeProperty(tileLoader, loggerFactory).DisposeItWith(Disposable);
    }

    public MapModeProperty MapModeProperty { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        yield return MapModeProperty;
    }
}
