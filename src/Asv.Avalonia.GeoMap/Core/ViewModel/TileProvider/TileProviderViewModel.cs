namespace Asv.Avalonia.GeoMap;

public class TileProviderViewModel(ITileProvider provider)
    : ViewModel(provider.Info.Id)
{
    public ITileProvider Provider { get; } = provider;
    public string Name { get; } = provider.Info.Name;
    public string Group => Provider.Info.Group.ToString();

    public override IEnumerable<IViewModel> GetChildren() => [];
}
