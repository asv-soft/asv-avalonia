using Asv.Avalonia;
using Material.Icons;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileProviderViewModel : HeadlinedViewModel
{
    public TileProviderViewModel(ITileProvider provider)
        : base(provider.Info.Id)
    {
        Provider = provider;
        Name = provider.Info.Name;
        Header = Name;
        Description = string.Format(
            RS.TileProviderViewModel_Description,
            Group,
            provider.Info.MinZoom,
            provider.Info.MaxZoom
        );
        Icon = MaterialIconKind.MapOutline;
        IconColor = GetIconColor(provider.Info.Group);
        IsCurrent = new BindableReactiveProperty<bool>(false).AddTo(ref DisposableBag);
    }

    public BindableReactiveProperty<bool> IsCurrent { get; }
    public ITileProvider Provider { get; }
    public string Name { get; }
    public string Group => Provider.Info.Group.ToString();

    public override IEnumerable<IViewModel> GetChildren() => [];

    private static AsvColorKind GetIconColor(TileProviderGroup group)
    {
        return group.Id switch
        {
            "OpenStreetMap" => AsvColorKind.Info3,
            "Google" => AsvColorKind.Info4,
            "Yandex" => AsvColorKind.Info5,
            "Bing" => AsvColorKind.Info6,
            "ArcGIS" => AsvColorKind.Info7,
            "HERE" => AsvColorKind.Info8,
            "Thunderforest" => AsvColorKind.Info9,
            _ => AsvColorKind.Info2,
        };
    }
}
