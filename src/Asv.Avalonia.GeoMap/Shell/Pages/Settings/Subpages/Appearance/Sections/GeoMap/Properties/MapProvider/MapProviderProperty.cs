using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class MapProviderProperty : RoutableViewModel
{
    public const string ViewModelId = "map-provider";

    public MapProviderProperty( /* IMapProvider, */
        ILoggerFactory loggerFactory
    )
        : base(ViewModelId, loggerFactory)
    {
        SelectedItem = new BindableReactiveProperty<string>("yandex");
    }

    public IEnumerable<string> Items => ["yandex", "bing"];
    public BindableReactiveProperty<string> SelectedItem { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }
}
