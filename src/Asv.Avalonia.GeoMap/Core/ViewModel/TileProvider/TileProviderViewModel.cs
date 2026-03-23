using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class TileProviderViewModel(ITileProvider provider, ILoggerFactory loggerFactory)
    : RoutableViewModel(provider.Info.Id, loggerFactory)
{
    public ITileProvider Provider { get; } = provider;
    public string Name { get; } = provider.Info.Name;
    public string Group => Provider.Info.Group.ToString();

    public override IEnumerable<IRoutable> GetChildren() => [];
}
