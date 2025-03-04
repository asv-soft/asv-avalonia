using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Asv.Avalonia.Map;
using Asv.Common;
using Material.Icons;

namespace Asv.Avalonia.Example;

public interface IMapContext : IPage { }

[ExportPage(PageId)]
public class MapExamplePageViewModel : PageViewModel<IMapContext>, IMapContext
{
    public const string PageId = "MapExamplePage";

    public MapExamplePageViewModel()
        : this(DesignTime.CommandService)
    {
        DesignTime.ThrowIfNotDesignMode();

        Title.OnNext(RS.MapExamplePageViewModel_Title);
    }

    [ImportingConstructor]
    public MapExamplePageViewModel(ICommandService cmd)
        : base(PageId, cmd)
    {
        Title.OnNext(RS.MapExamplePageViewModel_Title);
        Anchors = new MapViewModel("asdasd");
        Anchors.Anchors.Add(
            new MapAnchor("1")
            {
                Icon = MaterialIconKind.Navigation,
                Location = new GeoPoint(53, 53, 0),
            }
        );
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield break;
    }

    protected override IMapContext GetContext()
    {
        return this;
    }

    protected override void AfterLoadExtensions()
    {
        // do nothing
    }

    public override IExportInfo Source => SystemModule.Instance;
    public MapViewModel Anchors { get; }
}
